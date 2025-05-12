using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webApi.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthTestController : ControllerBase
    {
        [HttpGet("test")]
        [Authorize]
        public IActionResult TestAuth()
        {
            // Lấy thông tin user từ token
            var userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            return Ok(new
            {
                message = "Token hợp lệ!",
                userId,
                email
               
            });
        }
    }
} 