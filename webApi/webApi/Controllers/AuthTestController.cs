using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthTestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new { message = "This is a public endpoint. No authentication required." });
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult ProtectedEndpoint()
        {
            var userId = User.FindFirst("sub")?.Value;
            return Ok(new { 
                message = "This is a protected endpoint. Authentication required.",
                userId = userId
            });
        }

        [Authorize]
        [HttpGet("admin")]
        public IActionResult AdminEndpoint()
        {
            var userId = User.FindFirst("sub")?.Value;
            var email = User.FindFirst("email")?.Value;
            return Ok(new { 
                message = "This is an admin endpoint. Authentication required.",
                userId = userId,
                email = email
            });
        }
    }
} 