using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webApi.Model.CourseModel;
using webApi.Repositories;
using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class coursesApiController : ControllerBase
    {

        private readonly IcoursesRepository _coursesRepository;
        private readonly ApplicationDbContext _context;
        public coursesApiController(IcoursesRepository coursesRepository, ApplicationDbContext context)
        {
            _coursesRepository = coursesRepository;
            _context = context;
        }
        //Lây danh sách tất cả các khóa học
        [HttpGet]
        public async Task<IActionResult> Getcourses()
        {
            try
            {
                var courses = await _coursesRepository.GetcoursesAsync();
                return Ok(courses);
            }
            catch (Exception ex)
            {
                // Handle exception 
                return StatusCode(500, "Internal server error");
            }
        }
        //Lấy khóa học theo id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetcoursesById(int id)
        {
            try
            {
                var courses = await _coursesRepository.GetcoursesByIdAsync(id);
                if (courses == null)
                    return NotFound();

                return Ok(courses);
            }
            catch (Exception ex)
            {
                // Handle exception 
                return StatusCode(500, "Internal server error");
            }
        }
        //Thêm khóa học
        [HttpPost]
        public async Task<IActionResult> Addcourses([FromBody] courses courses)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _coursesRepository.AddcoursesAsync(courses);
                return CreatedAtAction(nameof(GetcoursesById), new { id = courses.Id }, courses);
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //Cập nhật khóa học
        [HttpPut("{id}")]
        public async Task<IActionResult> Updatecourses(int id, [FromBody] courses courses)
        {
            try
            {
                if (id != courses.Id)
                    return BadRequest();

                // Kiểm tra CategoryId nếu được cung cấp
                if (courses.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == courses.CategoryId.Value);
                    if (!categoryExists)
                    {
                        return BadRequest("Category not found");
                    }
                }

                await _coursesRepository.UpdatecoursesAsync(courses);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Handle exception 
                return StatusCode(500, "Internal server error");
            }
        }
        //Xóa khóa học
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletecourses(int id)
        {
            try
            {
                await _coursesRepository.DeletecoursesAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Handle exception 
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("free")]
        public async Task<IActionResult> GetFreecourses()
        {
            try
            {
                var freecourses = await _coursesRepository.GetFreecoursesAsync();
                return Ok(freecourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("recommend")]
        public async Task<IActionResult> GetRecommendedCourses([FromQuery] string userId, [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required");
                }

                var recommendations = await _coursesRepository.GetRecommendedCoursesAsync(userId, limit);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedCourses([FromQuery] int limit = 10)
        {
            try
            {
                var topRatedCourses = await _coursesRepository.GetTopRatedCoursesAsync(limit);
                return Ok(topRatedCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("{id}/related")]
        public async Task<IActionResult> AddRelatedCourse(int id, [FromBody] int relatedCourseId)
        {
            try
            {
                await _coursesRepository.AddRelatedCourseAsync(id, relatedCourseId);
                return Ok(new { message = "Related course added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}/related")]
        public async Task<IActionResult> GetRelatedCourses(int id)
        {
            try
            {
                var relatedCourses = await _coursesRepository.GetRelatedCoursesAsync(id);
                return Ok(relatedCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCourseStatus(int id, [FromBody] CourseStatus status)
        {
            try
            {
                await _coursesRepository.UpdateCourseStatusAsync(id, status);
                return Ok(new { message = "Course status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}/overview")]
        public async Task<IActionResult> GetCourseOverview(int id)
        {
            var overview = await _coursesRepository.GetCourseOverviewAsync(id);
            return Ok(overview);
        }
    }
}
