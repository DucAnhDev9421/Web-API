using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
                var course = await _context.courses
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return NotFound("Không tìm thấy khóa học");

                var response = new CourseResponseDto
                {
                    Id = course.Id,
                    Name = course.Name,
                    Price = course.Price,
                    Description = course.Description,
                    ImageUrl = course.ImageUrl,
                    VideoDemoUrl = course.VideoDemoUrl,
                    Status = (int)course.Status,
                    StatusText = course.StatusText,
                    Level = (int)course.Level,
                    LevelText = course.LevelText,
                    CategoryId = course.CategoryId,
                    Sections = course.Sections?.Select(s => new SectionResponseDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Lessons = s.Lessons?.Select(l => new LessonResponseDto
                        {
                            Id = l.Id,
                            Title = l.Title,
                            Type = (int)l.Type,
                            Content = l.Content
                        }).ToList()
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        //Thêm khóa học - Chỉ Admin và Instructor
        [HttpPost]
        public async Task<IActionResult> Addcourses([FromBody] CourseCreateDto dto)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (dto == null)
                    return BadRequest("Dữ liệu khóa học không được để trống");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest("Tên khóa học không được để trống");

                if (dto.Price < 0)
                    return BadRequest("Giá khóa học không được âm");

                if (string.IsNullOrWhiteSpace(dto.Description))
                    return BadRequest("Mô tả khóa học không được để trống");

                if (string.IsNullOrWhiteSpace(dto.ImageUrl))
                    return BadRequest("URL hình ảnh không được để trống");

                // Kiểm tra độ dài mô tả và URL hình ảnh
                if (dto.Description.Length > 500)
                    return BadRequest("Mô tả khóa học không được vượt quá 500 ký tự");

                if (dto.ImageUrl.Length > 500)
                    return BadRequest("URL hình ảnh không được vượt quá 500 ký tự");

                // Kiểm tra CategoryId nếu được cung cấp
                if (dto.CategoryId.HasValue)
                {
                    var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value);
                    if (!categoryExists)
                    {
                        return BadRequest("Danh mục không tồn tại");
                    }
                }

                // Kiểm tra trạng thái và cấp độ
                if (!Enum.IsDefined(typeof(CourseStatus), dto.Status))
                    return BadRequest("Trạng thái khóa học không hợp lệ");

                if (!Enum.IsDefined(typeof(CourseLevel), dto.Level))
                    return BadRequest("Cấp độ khóa học không hợp lệ");

                // Tạo entity khóa học kèm chương trình học
                var course = new courses
                {
                    Name = dto.Name,
                    Price = dto.Price,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    VideoDemoUrl = dto.VideoDemoUrl,
                    Status = (CourseStatus)dto.Status,
                    Level = (CourseLevel)dto.Level,
                    CategoryId = dto.CategoryId,
                    InstructorId = dto.InstructorId,
                    Sections = dto.Sections?.Select(s => new Section
                    {
                        Title = s.Title,
                        Lessons = s.Lessons?.Select(l => new Lesson
                        {
                            Title = l.Title,
                            Type = (LessonType)l.Type,
                            Content = l.Content
                        }).ToList()
                    }).ToList()
                };

                _context.courses.Add(course);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetcoursesById), new { id = course.Id }, new {
                    message = "Tạo khóa học thành công",
                    course = new CourseResponseDto
                    {
                        Id = course.Id,
                        Name = course.Name,
                        Price = course.Price,
                        Description = course.Description,
                        ImageUrl = course.ImageUrl,
                        VideoDemoUrl = course.VideoDemoUrl,
                        Status = (int)course.Status,
                        StatusText = course.StatusText,
                        Level = (int)course.Level,
                        LevelText = course.LevelText,
                        CategoryId = course.CategoryId,
                        Sections = course.Sections?.Select(s => new SectionResponseDto
                        {
                            Id = s.Id,
                            Title = s.Title,
                            Lessons = s.Lessons?.Select(l => new LessonResponseDto
                            {
                                Id = l.Id,
                                Title = l.Title,
                                Type = (int)l.Type,
                                Content = l.Content
                            }).ToList()
                        }).ToList()
                    }
                });
            }
            catch (DbUpdateException ex)
            {
                // Log lỗi database
                return StatusCode(500, "Lỗi khi lưu dữ liệu vào database: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Log lỗi khác
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
        //Cập nhật khóa học - Chỉ Admin và Instructor
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
        //Xóa khóa học - Chỉ Admin
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

        [HttpGet("{courseId}/next-video/{currentVideoId}")]
        public async Task<IActionResult> GetNextVideo(int courseId, int currentVideoId)
        {
            try
            {
                // Lấy khóa học với tất cả sections và lessons
                var course = await _context.courses
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return NotFound("Không tìm thấy khóa học");

                // Tìm lesson hiện tại
                var currentLesson = course.Sections
                    .SelectMany(s => s.Lessons)
                    .FirstOrDefault(l => l.Id == currentVideoId);

                if (currentLesson == null)
                    return NotFound("Không tìm thấy video hiện tại");

                // Tìm lesson tiếp theo
                var nextLesson = FindNextLesson(course.Sections, currentVideoId);

                if (nextLesson == null)
                    return Ok(new { 
                        message = "Đây là video cuối cùng của khóa học",
                        isLastVideo = true
                    });

                return Ok(new {
                    lessonId = nextLesson.Id,
                    title = nextLesson.Title,
                    content = nextLesson.Content,
                    type = (int)nextLesson.Type,
                    isLastVideo = false
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        private Lesson FindNextLesson(ICollection<Section> sections, int currentVideoId)
        {
            var allLessons = sections
                .OrderBy(s => s.Id)
                .SelectMany(s => s.Lessons.OrderBy(l => l.Id))
                .ToList();

            var currentIndex = allLessons.FindIndex(l => l.Id == currentVideoId);
            
            if (currentIndex == -1 || currentIndex == allLessons.Count - 1)
                return null;

            return allLessons[currentIndex + 1];
        }
    }
}
