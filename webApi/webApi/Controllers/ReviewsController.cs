using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews([FromQuery] int courseId)
        {
            try
            {
                var reviews = await _context.Ratings
                    .Include(r => r.User)
                    .Where(r => r.CourseId == courseId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => new
                    {
                        r.Id,
                        r.RatingValue,
                        r.Comment,
                        r.CreatedAt,
                        r.UpdatedAt,
                        User = new
                        {
                            r.User.Id,
                            r.User.Username,
                            r.User.ImageUrl
                        }
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetRatingStats()
        {
            try
            {
                var stats = await _context.Ratings
                    .GroupBy(r => r.CourseId)
                    .Select(g => new
                    {
                        CourseId = g.Key,
                        AverageRating = Math.Round(g.Average(r => r.RatingValue), 1),
                        TotalRatings = g.Count(),
                        RatingDistribution = new
                        {
                            FiveStars = g.Count(r => r.RatingValue == 5),
                            FourStars = g.Count(r => r.RatingValue == 4),
                            ThreeStars = g.Count(r => r.RatingValue == 3),
                            TwoStars = g.Count(r => r.RatingValue == 2),
                            OneStar = g.Count(r => r.RatingValue == 1)
                        }
                    })
                    .ToListAsync();

                // Get course names for the stats
                var courseIds = stats.Select(s => s.CourseId).ToList();
                var courses = await _context.courses
                    .Where(c => courseIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name })
                    .ToListAsync();

                // Combine stats with course names
                var result = stats.Select(s => new
                {
                    s.CourseId,
                    CourseName = courses.FirstOrDefault(c => c.Id == s.CourseId)?.Name ?? "Unknown Course",
                    s.AverageRating,
                    s.TotalRatings,
                    s.RatingDistribution
                })
                .OrderByDescending(s => s.AverageRating)
                .ThenByDescending(s => s.TotalRatings);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 