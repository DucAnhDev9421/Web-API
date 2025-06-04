using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using webApi.Model;
using System.Linq;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonProgressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LessonProgressController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class LessonProgressDto
        {
            public string UserId { get; set; }
            public int LessonId { get; set; }
        }

        // POST: Đánh dấu hoàn thành
        [HttpPost]
        public async Task<IActionResult> MarkCompleted([FromBody] LessonProgressDto dto)
        {
            if (string.IsNullOrEmpty(dto.UserId) || dto.LessonId <= 0)
                return BadRequest(new { message = "Invalid UserId or LessonId" });

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.LessonId == dto.LessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    UserId = dto.UserId,
                    LessonId = dto.LessonId,
                    IsCompleted = true,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Lesson marked as completed" });
        }

        // DELETE: Bỏ hoàn thành
        [HttpDelete]
        public async Task<IActionResult> UnmarkCompleted([FromBody] LessonProgressDto dto)
        {
            if (string.IsNullOrEmpty(dto.UserId) || dto.LessonId <= 0)
                return BadRequest(new { message = "Invalid UserId or LessonId" });

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.LessonId == dto.LessonId);

            if (progress == null)
            {
                return NotFound(new { message = "Progress not found" });
            }

            progress.IsCompleted = false;
            progress.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Lesson unmarked as completed" });
        }

        // GET: Lấy tiến độ học của user trong một khóa học
        [HttpGet]
        public async Task<IActionResult> GetProgress([FromQuery] string userId, [FromQuery] int courseId)
        {
            if (string.IsNullOrEmpty(userId) || courseId <= 0)
                return BadRequest(new { message = "Invalid UserId or CourseId" });

            // Lấy tất cả lessonId thuộc course
            var lessonIds = await _context.Lessons
                .Where(l => l.Section.CourseId == courseId)
                .Select(l => l.Id)
                .ToListAsync();

            var totalLessons = lessonIds.Count;

            // Lấy tiến độ của user với các lesson này
            var progresses = await _context.LessonProgresses
                .Where(p => p.UserId == userId && lessonIds.Contains(p.LessonId) && p.IsCompleted)
                .Select(p => p.LessonId)
                .ToListAsync();

            var completedCount = progresses.Count;
            var percent = totalLessons == 0 ? 0 : (completedCount * 100) / totalLessons;

            return Ok(new {
                completedLessons = progresses,
                completedCount,
                totalLessons,
                percentCompleted = percent
            });
        }
    }
} 