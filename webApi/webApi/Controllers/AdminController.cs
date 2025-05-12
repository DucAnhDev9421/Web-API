using Microsoft.AspNetCore.Mvc;
using webApi.Repositories;
using webApi.Model;

namespace webApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;

        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            try
            {
                var overview = await _adminRepository.GetOverviewAsync();
                return Ok(overview);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var overview = await _adminRepository.GetOverviewAsync();
                return Ok(new
                {
                    overview.CourseStats,
                    overview.UserStats,
                    overview.RatingStats,
                    // Add any additional stats needed for the dashboard
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 