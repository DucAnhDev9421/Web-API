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
                    .Select(r => new RatingResponseDto
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        FirstName = r.User.FirstName,
                        ImageUrl = r.User.ImageUrl,
                        RatingValue = r.RatingValue,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(reviews);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateRatingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra xem user đã đánh giá khóa học này chưa
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.CourseId == dto.CourseId);

                if (existingRating != null)
                {
                    return BadRequest("Bạn đã đánh giá khóa học này rồi");
                }

                // Kiểm tra xem user có tồn tại không
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null)
                {
                    return BadRequest("User không tồn tại");
                }

                // Kiểm tra xem khóa học có tồn tại không
                var course = await _context.courses.FindAsync(dto.CourseId);
                if (course == null)
                {
                    return BadRequest("Khóa học không tồn tại");
                }

                var rating = new Rating
                {
                    UserId = dto.UserId,
                    CourseId = dto.CourseId,
                    RatingValue = dto.RatingValue,
                    Comment = dto.Comment,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Ratings.Add(rating);
                await _context.SaveChangesAsync();

                var response = new RatingResponseDto
                {
                    Id = rating.Id,
                    UserId = rating.UserId,
                    FirstName = user.FirstName,
                    ImageUrl = user.ImageUrl,
                    RatingValue = rating.RatingValue,
                    Comment = rating.Comment,
                    CreatedAt = rating.CreatedAt,
                    UpdatedAt = rating.UpdatedAt
                };

                return CreatedAtAction(nameof(GetReviews), new { courseId = rating.CourseId }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var rating = await _context.Ratings.FindAsync(id);
                if (rating == null)
                {
                    return NotFound("Không tìm thấy đánh giá");
                }

                _context.Ratings.Remove(rating);
                await _context.SaveChangesAsync();

                return NoContent();
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

        [HttpGet("stats/{courseId}")]
        public async Task<IActionResult> GetRatingStatsByCourse(int courseId)
        {
            try
            {
                var stats = await _context.Ratings
                    .Where(r => r.CourseId == courseId)
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
                    .FirstOrDefaultAsync();

                if (stats == null)
                    return NotFound();

                var course = await _context.courses.FindAsync(courseId);
                return Ok(new
                {
                    stats.CourseId,
                    CourseName = course?.Name ?? "Unknown Course",
                    stats.AverageRating,
                    stats.TotalRatings,
                    stats.RatingDistribution
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 