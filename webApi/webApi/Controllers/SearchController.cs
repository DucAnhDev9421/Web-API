using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.CourseModel;
using webApi.Model.CategoryModel;

namespace webApi.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest("Search query is required");
                }

                var query = q.ToLower().Trim();

                // Search in courses with additional information
                var courses = await _context.courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .Include(c => c.Ratings)
                    .Where(c => c.Status == CourseStatus.Approved &&
                        (c.Name.ToLower().Contains(query) ||
                         (c.Description != null && c.Description.ToLower().Contains(query))))
                    .Select(c => new
                    {
                        Type = "course",
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Price = c.Price,
                        ImageUrl = c.ImageUrl,
                        Status = c.Status,
                        StatusText = c.StatusText,
                        Level = c.Level,
                        LevelText = c.LevelText,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category != null ? c.Category.Name : null,
                        Instructor = c.Instructor != null ? new
                        {
                            Id = c.Instructor.Id,
                            Username = c.Instructor.FirstName,
                            ImageUrl = c.Instructor.ImageUrl
                        } : null,
                        // Thêm thông tin đánh giá và số học viên
                        AverageRating = c.Ratings.Any() ? Math.Round(c.Ratings.Average(r => r.RatingValue), 1) : 0,
                        TotalRatings = c.Ratings.Count,
                        EnrollmentCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
                    })
                    .ToListAsync();

                // Search in categories
                var categories = await _context.Categories
                    .Where(c => c.Name.ToLower().Contains(query))
                    .Select(c => new
                    {
                        Type = "category",
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        CourseCount = _context.courses.Count(co => co.CategoryId == c.Id)
                    })
                    .ToListAsync();

                // Combine results
                var results = new
                {
                    query = q,
                    totalResults = courses.Count + categories.Count,
                    courses = new
                    {
                        total = courses.Count,
                        items = courses
                    },
                    categories = new
                    {
                        total = categories.Count,
                        items = categories
                    }
                };

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("suggestion")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string q, [FromQuery] int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                {
                    return Ok(new { suggestions = new List<object>() });
                }
                var query = q.ToLower().Trim();

                // Lấy gợi ý từ tên khóa học
                var courseSuggestions = await _context.courses
                    .Where(c => c.Status == CourseStatus.Approved && c.Name.ToLower().Contains(query))
                    .Select(c => new {
                        Id = c.Id,
                        Name = c.Name,
                        Type = "course",
                        ImageUrl = c.ImageUrl,
                        Instructor = c.Instructor != null ? new {
                            Id = c.Instructor.Id,
                            Name = c.Instructor.FirstName,
                            ImageUrl = c.Instructor.ImageUrl
                        } : null
                    })
                    .Take(limit)
                    .ToListAsync();

                // Lấy gợi ý từ tên danh mục
                var categorySuggestions = await _context.Categories
                    .Where(c => c.Name.ToLower().Contains(query))
                    .Select(c => new {
                        Id = c.Id,
                        Name = c.Name,
                        Type = "category",
                        CourseCount = _context.courses.Count(co => co.CategoryId == c.Id)
                    })
                    .Take(limit)
                    .ToListAsync();

                // Gộp và sắp xếp kết quả, ưu tiên khóa học trước
                var suggestions = courseSuggestions
                    .Cast<object>()
                    .Concat(categorySuggestions.Cast<object>())
                    .Take(limit)
                    .ToList();

                return Ok(new { 
                    suggestions,
                    total = suggestions.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 