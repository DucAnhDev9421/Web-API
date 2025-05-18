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
            return Ok(new
            {
                message = "Token hợp lệ!"
            });
        }
    }
} 