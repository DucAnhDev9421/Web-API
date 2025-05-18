using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webApi.Repositories;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyEnrollmentStats()
        {
            var stats = await _dashboardRepository.GetWeeklyEnrollmentStatsAsync();
            return Ok(stats);
        }
    }
} 