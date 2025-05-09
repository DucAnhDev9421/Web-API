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

                // Lưu email từ email_addresses
                if (userData.EmailAddresses != null && userData.EmailAddresses.Any())
                {
                    userData.Email = userData.EmailAddresses[0].Email;
                }

                // Lưu vào database
                await _userRepository.CreateOrUpdateUserAsync(userData);
                
                return Ok(userData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
} 