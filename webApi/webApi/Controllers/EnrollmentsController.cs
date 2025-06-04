using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using webApi.Repositories;
using System.Linq;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        public EnrollmentsController(IEnrollmentRepository enrollmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
        }

        public class EnrollRequest
        {
            public string UserId { get; set; }
            public int CourseId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || request.CourseId <= 0)
                return BadRequest(new { message = "Invalid userId or courseId" });

            var result = await _enrollmentRepository.EnrollAsync(request.UserId, request.CourseId);
            if (!result)
                return Conflict(new { message = "User already enrolled in this course" });

            return Ok(new { message = "Enrollment successful" });
        }

        [HttpGet]
        public async Task<IActionResult> GetEnrollments([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "Missing userId" });

            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserAsync(userId);
            var result = enrollments.Select(e => new {
                enrollmentId = e.Id,
                courseId = e.CourseId,
                enrolledAt = e.EnrolledAt,
                course = e.Course != null ? new {
                    e.Course.Id,
                    e.Course.Name,
                    e.Course.Description,
                    e.Course.StatusText,
                    e.Course.ImageUrl
                } : null
            });
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Unenroll([FromBody] EnrollRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || request.CourseId <= 0)
                return BadRequest(new { message = "Invalid userId or courseId" });

            var result = await _enrollmentRepository.UnenrollAsync(request.UserId, request.CourseId);
            if (!result)
                return NotFound(new { message = "Enrollment not found" });

            return Ok(new { message = "Unenrollment successful" });
        }
    }
} 