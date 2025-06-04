using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using webApi.Repositories;
using webApi.Model;

namespace webApi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IcoursesRepository _coursesRepository;

        public AdminController(IAdminRepository adminRepository, IcoursesRepository coursesRepository)
        {
            _adminRepository = adminRepository;
            _coursesRepository = coursesRepository;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }
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
                var stats = await _coursesRepository.GetDashboardStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 