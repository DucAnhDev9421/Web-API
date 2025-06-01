using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using webApi.Model;
using webApi.Model.UserModel;
using webApi.Repositories;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace webApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IUserCourseProgressRepository _userCourseProgressRepository;
        private readonly INoteRepository _noteRepository;
        private readonly ApplicationDbContext _context;

        public UsersController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IUserRepository userRepository,
            IUserCourseProgressRepository userCourseProgressRepository,
            INoteRepository noteRepository,
            ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
            _userCourseProgressRepository = userCourseProgressRepository;
            _noteRepository = noteRepository;
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                // Lấy thông tin từ Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");
                
                var response = await client.GetAsync($"users/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không tìm thấy thông tin người dùng từ Clerk");
                }

                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserInfo>(content);

                // Đảm bảo ID được gán
                userData.Id = id;

                // Lưu email từ email_addresses
                if (userData.EmailAddresses != null && userData.EmailAddresses.Any())
                {
                    userData.Email = userData.EmailAddresses[0].Email;
                }
                else
                {
                    // Nếu không có email, gán giá trị mặc định
                    userData.Email = $"{id}@temp.com";
                }

                // Lấy role từ private_metadata
                userData.Role = userData.PrivateMetadata?.Role ?? "user";

                // Đảm bảo các trường bắt buộc không null
                userData.Username ??= id;
                userData.FirstName ??= string.Empty;
                userData.LastName ??= string.Empty;
                userData.ImageUrl ??= string.Empty;
                userData.ProfileImageUrl ??= string.Empty;

                // Lưu vào database
                try
                {
                    await _userRepository.CreateOrUpdateUserAsync(userData);
                }
                catch (Exception ex)
                {
                    // Log lỗi chi tiết
                    Console.WriteLine($"Lỗi khi lưu user vào database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
                
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                // Kiểm tra trong database
                var userFromDb = await _userRepository.GetUserByIdAsync(id);
                if (userFromDb == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật thông tin từ Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");

                // Tạo request body cho Clerk API
                var updateData = new
                {
                    username = updateDto.Username,
                    first_name = updateDto.FirstName,
                    last_name = updateDto.LastName,
                    image_url = updateDto.ImageUrl,
                    profile_image_url = updateDto.ProfileImageUrl
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PatchAsync($"users/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không thể cập nhật thông tin người dùng trên Clerk");
                }

                // Cập nhật thông tin trong database
                userFromDb.Username = updateDto.Username;
                userFromDb.FirstName = updateDto.FirstName;
                userFromDb.LastName = updateDto.LastName;
                userFromDb.ImageUrl = updateDto.ImageUrl;
                userFromDb.ProfileImageUrl = updateDto.ProfileImageUrl;
                userFromDb.UpdatedAt = DateTime.UtcNow;

                await _userRepository.CreateOrUpdateUserAsync(userFromDb);

                return Ok(userFromDb);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra trong database
                var userFromDb = await _userRepository.GetUserByIdAsync(id);
                if (userFromDb == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật mật khẩu trên Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");

                // Tạo request body cho Clerk API
                var updateData = new
                {
                    current_password = changePasswordDto.CurrentPassword,
                    new_password = changePasswordDto.NewPassword
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                // Sửa lại endpoint đổi mật khẩu
                var response = await client.PutAsync($"users/{id}/change_password", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, 
                        $"Không thể đổi mật khẩu: {errorContent}");
                }

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/dashboard")]
        public async Task<IActionResult> GetUserDashboard(string id)
        {
            try
            {
                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var dashboard = await _userCourseProgressRepository.GetUserDashboardAsync(id);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/profile")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                return Ok(new {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    imageUrl = user.ImageUrl,
                    jobTitle = user.JobTitle,
                    bio = user.Bio
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPatch("{id}/favorite")]
        public async Task<IActionResult> ToggleFavoriteCourse(string id, [FromBody] FavoriteCourseDto favoriteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var isNowFavorite = await _userRepository.ToggleFavoriteCourseAsync(id, favoriteDto.CourseId);
                
                return Ok(new { 
                    isFavorite = isNowFavorite,
                    message = isNowFavorite 
                        ? "Đã thêm khóa học vào danh sách yêu thích" 
                        : "Đã xóa khóa học khỏi danh sách yêu thích"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/favorites")]
        public async Task<IActionResult> GetFavoriteCourses(string id)
        {
            try
            {
                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                var favorites = await _userRepository.GetUserFavoriteCoursesAsync(id);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetUserNotes(string id)
        {
            try
            {
                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Lấy danh sách ghi chú của user
                var notes = await _noteRepository.GetNotesByUserIdAsync(id);
                
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("{id}/notes")]
        public async Task<IActionResult> CreateUserNote(string id, [FromBody] CreateUserNoteDto createNoteDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Kiểm tra video tồn tại (nếu cần)
                // Có thể thêm kiểm tra video tồn tại ở đây nếu cần

                // Tạo ghi chú mới
                var note = new Note
                {
                    Title = createNoteDto.Title,
                    Content = createNoteDto.Content,
                    UserId = id,
                    CreatedAt = DateTime.UtcNow
                };

                // Lưu ghi chú vào database
                await _noteRepository.AddNoteAsync(note);

                return CreatedAtAction(nameof(GetUserNotes), new { id = id }, note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetUserStatistics(string id)
        {
            try
            {
                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Lấy thống kê chi tiết
                var statistics = await _userCourseProgressRepository.GetUserStatisticsAsync(id);
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetAllCourseProgress(string id)
        {
            var result = await _userRepository.GetAllCourseProgressAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/role")]
        public async Task<IActionResult> GetUserRole(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                return Ok(new { role = user.Role ?? "user" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPatch("{id}/profile")]
        public async Task<IActionResult> UpdateUserProfile(string id, [FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra user tồn tại
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật thông tin từ Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");

                // Tạo request body cho Clerk API
                var updateData = new
                {
                    first_name = updateDto.FirstName,
                    last_name = updateDto.LastName,
                    public_metadata = new {
                        jobTitle = updateDto.JobTitle,
                        bio = updateDto.Bio
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PatchAsync($"users/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không thể cập nhật thông tin người dùng trên Clerk");
                }

                // Cập nhật thông tin trong database
                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.JobTitle = updateDto.JobTitle;
                user.Bio = updateDto.Bio;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.CreateOrUpdateUserAsync(user);

                return Ok(new {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    jobTitle = user.JobTitle,
                    bio = user.Bio,
                    message = "Cập nhật thông tin thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet("instructor/{id}")]
        public async Task<IActionResult> GetInstructorInfo(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("Không tìm thấy thông tin giảng viên");
                }

                var instructorInfo = new InstructorInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    ImageUrl = user.ImageUrl
                };

                return Ok(instructorInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("{id}/sync")]
        public async Task<IActionResult> SyncUserData(string id)
        {
            try
            {
                // Lấy thông tin từ Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");
                
                var response = await client.GetAsync($"users/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không tìm thấy thông tin người dùng từ Clerk");
                }

                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserInfo>(content);

                // Đảm bảo ID được gán
                userData.Id = id;

                // Lưu email từ email_addresses
                if (userData.EmailAddresses != null && userData.EmailAddresses.Any())
                {
                    userData.Email = userData.EmailAddresses[0].Email;
                }
                else
                {
                    userData.Email = $"{id}@temp.com";
                }

                // Lấy role từ private_metadata
                userData.Role = userData.PrivateMetadata?.Role ?? "user";

                // Đảm bảo các trường bắt buộc không null
                userData.Username ??= id;
                userData.FirstName ??= string.Empty;
                userData.LastName ??= string.Empty;
                userData.ImageUrl ??= string.Empty;
                userData.ProfileImageUrl ??= string.Empty;

                // Cập nhật vào database
                await _userRepository.CreateOrUpdateUserAsync(userData);

                // Nếu người dùng là instructor, trả về thêm thông tin instructor
                if (userData.Role?.ToLower() == "instructor")
                {
                    var instructorInfo = new InstructorInfo
                    {
                        Id = userData.Id,
                        Username = userData.Username,
                        ImageUrl = userData.ImageUrl
                    };

                    return Ok(new { 
                        message = "Đồng bộ thông tin người dùng thành công",
                        user = userData,
                        instructor = instructorInfo
                    });
                }

                return Ok(new { 
                    message = "Đồng bộ thông tin người dùng thành công",
                    user = userData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra trong database
                var userFromDb = await _userRepository.GetUserByIdAsync(id);
                if (userFromDb == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                // Cập nhật role trong Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");

                // Tạo request body cho Clerk API
                var updateData = new
                {
                    private_metadata = new
                    {
                        role = updateRoleDto.Role
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(updateData),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PatchAsync($"users/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không thể cập nhật role người dùng trên Clerk");
                }

                // Cập nhật role trong database
                userFromDb.Role = updateRoleDto.Role;
                userFromDb.UpdatedAt = DateTime.UtcNow;

                await _userRepository.CreateOrUpdateUserAsync(userFromDb);

                return Ok(new { 
                    message = "Cập nhật role thành công",
                    role = userFromDb.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpPost("{id}/sync-role")]
        public async Task<IActionResult> SyncUserRole(string id)
        {
            try
            {
                // Lấy thông tin từ Clerk
                var client = _httpClientFactory.CreateClient("Clerk");
                var secretKey = _configuration["Clerk:SecretKey"];
                
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {secretKey}");
                
                var response = await client.GetAsync($"users/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Không tìm thấy thông tin người dùng từ Clerk");
                }

                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserInfo>(content);

                // Lấy role từ public_metadata
                var role = userData.PublicMetadata?.Role ?? "user";

                // Kiểm tra trong database
                var userFromDb = await _userRepository.GetUserByIdAsync(id);
                if (userFromDb == null)
                {
                    return NotFound("Không tìm thấy người dùng trong database");
                }

                // Cập nhật role trong database
                userFromDb.Role = role;
                userFromDb.UpdatedAt = DateTime.UtcNow;

                await _userRepository.CreateOrUpdateUserAsync(userFromDb);

                return Ok(new { 
                    message = "Đồng bộ role thành công",
                    role = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Enrollments)
                    .Include(u => u.Courses)
                        .ThenInclude(c => c.Ratings)
                    .ToListAsync();

                var userList = new List<object>();

                foreach (var user in users)
                {
                    if (user.Role?.ToLower() == "instructor")
                    {
                        // Thông tin cho giảng viên
                        var instructorInfo = new
                        {
                            id = user.Id,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            imageUrl = user.ImageUrl,
                            role = user.Role,
                            joinedAt = user.CreatedAt,
                            courses = new
                            {
                                total = user.Courses?.Count ?? 0,
                                students = user.Courses?.Sum(c => c.Enrollments?.Count ?? 0) ?? 0,
                                rating = user.Courses?.Any() == true 
                                    ? Math.Round(user.Courses.Average(c => c.Ratings?.Average(r => r.RatingValue) ?? 0), 1)
                                    : 0
                            }
                        };
                        userList.Add(instructorInfo);
                    }
                    else
                    {
                        // Thông tin cho người dùng thông thường
                        var userInfo = new
                        {
                            id = user.Id,
                            firstName = user.FirstName,
                            lastName = user.LastName,
                            imageUrl = user.ImageUrl,
                            role = user.Role ?? "user",
                            joinedAt = user.CreatedAt,
                            enrolledCourses = user.Enrollments?.Count ?? 0
                        };
                        userList.Add(userInfo);
                    }
                }

                return Ok(userList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }

    public class FavoriteCourseDto
    {
        public int CourseId { get; set; }
    }

    public class UserUpdateDto
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Role { get; set; }
    }

    public class CreateUserNoteDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string Content { get; set; }

        [Required(ErrorMessage = "VideoId không được để trống")]
        public int VideoId { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Họ không được để trống")]
        public string LastName { get; set; }

        public string JobTitle { get; set; }
        public string Bio { get; set; }
    }

    public class InstructorInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Role không được để trống")]
        public string Role { get; set; }
    }
} 