﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using webApi.Model.CourseModel;
using webApi.Repositories;
using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Services;
using System.Text.Json;

namespace webApi.Controllers
{
    [Route("api/courses")]
    [ApiController]
    public class coursesApiController : ControllerBase
    {

        private readonly IcoursesRepository _coursesRepository;
        private readonly ApplicationDbContext _context;
        private readonly IYouTubeService _youtubeService;
        public coursesApiController(IcoursesRepository coursesRepository, ApplicationDbContext context, IYouTubeService youtubeService)
        {
            _coursesRepository = coursesRepository;
            _context = context;
            _youtubeService = youtubeService;
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
                    .Include(c => c.Instructor)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return NotFound("Không tìm thấy khóa học");

                // Tính tổng thời lượng video
                var totalSeconds = course.Sections?
                    .SelectMany(s => s.Lessons)
                    .Where(l => l.Type == (int)LessonType.Video && !string.IsNullOrEmpty(l.Duration))
                    .Sum(l => ParseDurationToSeconds(l.Duration)) ?? 0;

                // Tính thông tin đánh giá
                var ratings = await _context.Ratings
                    .Where(r => r.CourseId == id)
                    .ToListAsync();

                var averageRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0;
                var totalRatings = ratings.Count;

                // Tính số học viên đăng ký
                var enrollmentCount = await _context.Enrollments
                    .CountAsync(e => e.CourseId == id);

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
                    TotalDuration = FormatTotalDuration(totalSeconds),
                    Instructor = course.Instructor != null ? new webApi.Model.CourseModel.InstructorInfo
                    {
                        Id = course.Instructor.Id,
                        Username = course.Instructor.FirstName,
                        ImageUrl = course.Instructor.ImageUrl
                    } : null,
                    Sections = course.Sections?.Select(s => new SectionResponseDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        Lessons = s.Lessons?.Select(l => new LessonResponseDto
                        {
                            Id = l.Id,
                            Title = l.Title,
                            Type = (int)l.Type,
                            Content = l.Content,
                            Duration = l.Duration
                        }).ToList()
                    }).ToList(),
                    Topics = !string.IsNullOrEmpty(course.Topics) ? JsonSerializer.Deserialize<List<string>>(course.Topics) : new List<string>(),
                    // Thêm thông tin mới
                    AverageRating = Math.Round(averageRating, 1),
                    TotalRatings = totalRatings,
                    EnrollmentCount = enrollmentCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        private int ParseDurationToSeconds(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;

            var parts = duration.Split(':');
            if (parts.Length == 2) // MM:SS format
            {
                if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                {
                    return minutes * 60 + seconds;
                }
            }
            else if (parts.Length == 3) // HH:MM:SS format
            {
                if (int.TryParse(parts[0], out int hours) && 
                    int.TryParse(parts[1], out int minutes) && 
                    int.TryParse(parts[2], out int seconds))
                {
                    return hours * 3600 + minutes * 60 + seconds;
                }
            }
            return 0;
        }

        private string FormatTotalDuration(int totalSeconds)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            if (hours > 0)
            {
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
            else
            {
                return $"{minutes:D2}:{seconds:D2}";
            }
        }
        //Lấy danh sách khóa học theo ID giảng viên
        [HttpGet("instructor/{instructorId}")]
        public async Task<IActionResult> GetCoursesByInstructor(string instructorId)
        {
            try
            {
                var courses = await _context.courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .Where(c => c.InstructorId == instructorId)
                    .Select(c => new CourseWithCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Price = c.Price,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        Status = c.Status,
                        StatusText = c.StatusText,
                        Level = c.Level,
                        LevelText = c.LevelText,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category != null ? c.Category.Name : null,
                        Instructor = c.Instructor != null ? new webApi.Model.CourseModel.InstructorInfo
                        {
                            Id = c.Instructor.Id,
                            Username = c.Instructor.FirstName,
                            ImageUrl = c.Instructor.ImageUrl
                        } : null
                    })
                    .ToListAsync();

                return Ok(courses);
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
                        Lessons = s.Lessons?.Select(async l => 
                        {
                            var lesson = new Lesson
                            {
                                Title = l.Title,
                                Type = (LessonType)l.Type,
                                Content = l.Content
                            };

                            // Nếu là video YouTube, lấy thời lượng
                            if (l.Type == (int)LessonType.Video && !string.IsNullOrEmpty(l.Content))
                            {
                                try
                                {
                                    lesson.Duration = await _youtubeService.GetVideoDurationAsync(l.Content);
                                }
                                catch (Exception ex)
                                {
                                    // Log error but continue
                                    Console.WriteLine($"Error getting video duration: {ex.Message}");
                                }
                            }

                            return lesson;
                        }).Select(t => t.Result).ToList()
                    }).ToList(),
                    Topics = dto.Topics != null ? System.Text.Json.JsonSerializer.Serialize(dto.Topics) : null
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
                        }).ToList(),
                        Topics = !string.IsNullOrEmpty(course.Topics) ? System.Text.Json.JsonSerializer.Deserialize<List<string>>(course.Topics) : new List<string>()
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
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto updateDto)
        {
            try
            {
                var course = await _context.courses
                    .Include(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                {
                    return NotFound("Course not found");
                }

                // Update basic course information
                course.Name = updateDto.Name;
                course.Price = updateDto.Price;
                course.Description = updateDto.Description;
                course.ImageUrl = updateDto.ImageUrl;
                course.VideoDemoUrl = updateDto.VideoDemoUrl;
                course.Status = (CourseStatus)updateDto.Status;
                course.Level = (CourseLevel)updateDto.Level;
                course.CategoryId = updateDto.CategoryId;
                course.InstructorId = updateDto.InstructorId;

                // Update sections and lessons
                if (updateDto.Sections != null)
                {
                    // Remove existing sections and lessons
                    _context.Sections.RemoveRange(course.Sections);

                    // Add new sections and lessons
                    foreach (var sectionDto in updateDto.Sections)
                    {
                        var section = new Section
                        {
                            Title = sectionDto.Title,
                            CourseId = id,
                            Lessons = sectionDto.Lessons?.Select(l => new Lesson
                            {
                                Title = l.Title,
                                Type = (LessonType)l.Type,
                                Content = l.Content,
                                Duration = l.Duration
                            }).ToList()
                        };
                        course.Sections.Add(section);
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Course updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                var relatedCourses = await _context.courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .Where(c => _context.RelatedCourses
                        .Where(rc => rc.CourseId == id)
                        .Select(rc => rc.RelatedCourseId)
                        .Contains(c.Id))
                    .Select(c => new CourseWithCategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Price = c.Price,
                        Description = c.Description,
                        ImageUrl = c.ImageUrl,
                        Status = c.Status,
                        StatusText = c.StatusText,
                        Level = c.Level,
                        LevelText = c.LevelText,
                        CategoryId = c.CategoryId,
                        CategoryName = c.Category != null ? c.Category.Name : null,
                        Instructor = c.Instructor != null ? new webApi.Model.CourseModel.InstructorInfo
                        {
                            Id = c.Instructor.Id,
                            Username = c.Instructor.FirstName,
                            ImageUrl = c.Instructor.ImageUrl
                        } : null
                    })
                    .ToListAsync();

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

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var popularCourses = await _context.courses
                    .Include(c => c.Instructor)
                    .Include(c => c.Category)
                    .Select(c => new
                    {
                        Course = c,
                        AverageRating = _context.Ratings
                            .Where(r => r.CourseId == c.Id)
                            .Average(r => (double?)r.RatingValue) ?? 0,
                        EnrollmentCount = _context.Enrollments
                            .Count(e => e.CourseId == c.Id)
                    })
                    .OrderByDescending(x => x.AverageRating * 0.7 + x.EnrollmentCount * 0.3) // Weighted scoring
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new CourseWithCategoryDto
                    {
                        Id = x.Course.Id,
                        Name = x.Course.Name,
                        Price = x.Course.Price,
                        Description = x.Course.Description,
                        ImageUrl = x.Course.ImageUrl,
                        Status = x.Course.Status,
                        StatusText = x.Course.StatusText,
                        Level = x.Course.Level,
                        LevelText = x.Course.LevelText,
                        CategoryId = x.Course.CategoryId,
                        CategoryName = x.Course.Category != null ? x.Course.Category.Name : null,
                        Instructor = x.Course.Instructor != null ? new webApi.Model.CourseModel.InstructorInfo
                        {
                            Id = x.Course.Instructor.Id,
                            Username = x.Course.Instructor.FirstName,
                            ImageUrl = x.Course.Instructor.ImageUrl
                        } : null,
                        AverageRating = Math.Round(x.AverageRating, 1),
                        EnrollmentCount = x.EnrollmentCount
                    })
                    .ToListAsync();

                // Get total count for pagination
                var totalCount = await _context.courses.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                return Ok(new
                {
                    Courses = popularCourses,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalPages = totalPages,
                        TotalItems = totalCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
