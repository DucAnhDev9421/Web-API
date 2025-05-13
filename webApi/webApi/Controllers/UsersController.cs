using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using webApi.Model;
using webApi.Model.UserModel;
using webApi.Repositories;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

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

        public UsersController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IUserRepository userRepository,
            IUserCourseProgressRepository userCourseProgressRepository,
            INoteRepository noteRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
            _userCourseProgressRepository = userCourseProgressRepository;
            _noteRepository = noteRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                // Kiểm tra trong database trước
                var userFromDb = await _userRepository.GetUserByIdAsync(id);
                if (userFromDb != null)
                {
                    return Ok(userFromDb);
                }

                // Nếu không có trong database, lấy từ Clerk
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
                var profile = await _userRepository.GetUserProfileAsync(id);
                if (profile == null)
                {
                    return NotFound("Không tìm thấy người dùng");
                }

                return Ok(profile);
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
                    VideoId = createNoteDto.VideoId,
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
    }

    public class FavoriteCourseDto
    {
        public int CourseId { get; set; }
    }

    public class CreateUserNoteDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string Content { get; set; }

        [Required(ErrorMessage = "VideoId không được để trống")]
        public int VideoId { get; set; }
    }
} 