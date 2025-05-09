using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using webApi.Model;
using webApi.Repositories;

namespace webApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public UsersController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            IUserRepository userRepository)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
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
    }
} 