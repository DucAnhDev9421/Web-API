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

                // Search in courses
                var courses = await _context.courses
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
                        ImageUrl = c.ImageUrl
                    })
                    .ToListAsync();

                // Search in categories
                var categories = await _context.Categories
                    .Where(c => c.Name.ToLower().Contains(query))
                    .Select(c => new
                    {
                        Type = "category",
                        Id = c.Id,
                        Name = c.Name
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
                    return Ok(new { suggestions = new List<string>() });
                }
                var query = q.ToLower().Trim();

                // Lấy gợi ý từ tên khóa học
                var courseSuggestions = await _context.courses
                    .Where(c => c.Status == CourseStatus.Approved && c.Name.ToLower().Contains(query))
                    .Select(c => c.Name)
                    .Distinct()
                    .Take(limit)
                    .ToListAsync();

                // Lấy gợi ý từ tên danh mục
                var categorySuggestions = await _context.Categories
                    .Where(c => c.Name.ToLower().Contains(query))
                    .Select(c => c.Name)
                    .Distinct()
                    .Take(limit)
                    .ToListAsync();

                // Gộp và loại trùng, ưu tiên tên khóa học trước
                var suggestions = courseSuggestions
                    .Concat(categorySuggestions)
                    .Distinct()
                    .Take(limit)
                    .ToList();

                return Ok(new { suggestions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 