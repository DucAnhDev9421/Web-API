using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webApi.Model;
using webApi.Repositories;

namespace webApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class coursesApiController : ControllerBase
    {

        private readonly IcoursesRepository _coursesRepository;
        public coursesApiController(IcoursesRepository coursesRepository)
        {
            _coursesRepository = coursesRepository;
        }
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Updatecourses(int id, [FromBody] courses courses)
        {
            try
            {
                if (id != courses.Id)
                    return BadRequest();
                await _coursesRepository.UpdatecoursesAsync(courses);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Handle exception 
                return StatusCode(500, "Internal server error");
            }
        }
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
    }
}
