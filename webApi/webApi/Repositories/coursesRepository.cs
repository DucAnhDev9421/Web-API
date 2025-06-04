using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.CourseModel;
using webApi.Model.UserModel;

namespace webApi.Repositories
{
    public class coursesRepository : IcoursesRepository
    {
        private readonly ApplicationDbContext _context;

        public coursesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseWithCategoryDto>> GetcoursesAsync()
        {
            var courses = await _context.courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .ToListAsync();

            var result = courses.Select(c => {
                // Tính tổng thời lượng video
                var totalSeconds = c.Sections?
                    .SelectMany(s => s.Lessons)
                    .Where(l => l.Type == (int)LessonType.Video && !string.IsNullOrEmpty(l.Duration))
                    .Sum(l => ParseDurationToSeconds(l.Duration)) ?? 0;

                // Tính số lượng đăng ký
                var enrollmentCount = _context.Enrollments.Count(e => e.CourseId == c.Id);

                // Tính đánh giá sao trung bình
                var ratings = _context.Ratings.Where(r => r.CourseId == c.Id).ToList();
                double avgRating = ratings.Any() ? Math.Round(ratings.Average(r => r.RatingValue), 1) : 0;

                return new CourseWithCategoryDto
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
                    Instructor = c.Instructor != null ? new InstructorInfo
                    {
                        Id = c.Instructor.Id,
                        Username = c.Instructor.FirstName,
                        ImageUrl = c.Instructor.ImageUrl
                    } : null,
                    AverageRating = avgRating,
                    EnrollmentCount = enrollmentCount,
                    TotalDuration = FormatTotalDuration(totalSeconds)
                };
            }).ToList();

            return result;
        }

        private int ParseDurationToSeconds(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;
            var parts = duration.Split(':');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int minutes) && int.TryParse(parts[1], out int seconds))
                {
                    return minutes * 60 + seconds;
                }
            }
            else if (parts.Length == 3)
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

        public async Task<CourseWithCategoryDto> GetcoursesByIdAsync(int id)
        {
            var course = await _context.courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return null;

            return new CourseWithCategoryDto
            {
                Id = course.Id,
                Name = course.Name,
                Price = course.Price,
                Description = course.Description,
                ImageUrl = course.ImageUrl,
                Status = course.Status,
                StatusText = course.StatusText,
                Level = course.Level,
                LevelText = course.LevelText,
                CategoryId = course.CategoryId,
                CategoryName = course.Category != null ? course.Category.Name : null,
                Instructor = course.Instructor != null ? new InstructorInfo
                {
                    Id = course.Instructor.Id,
                    Username = course.Instructor.FirstName,
                    ImageUrl = course.Instructor.ImageUrl
                } : null
            };
        }

        public async Task AddcoursesAsync(courses courses)
        {
            _context.courses.Add(courses);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatecoursesAsync(courses courses)
        {
            _context.Entry(courses).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeletecoursesAsync(int id)
        {
            var courses = await _context.courses.FindAsync(id);
            if (courses != null)
            {
                _context.courses.Remove(courses);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<courses>> GetFreecoursesAsync()
        {
            return await _context.courses
                .Where(c => c.Price == 0)  //  Lọc các khóa học có giá bằng 0
                .ToListAsync();
        }

        public async Task<IEnumerable<CourseRecommendation>> GetRecommendedCoursesAsync(string userId, int limit = 5)
        {
            // Lấy danh sách khóa học người dùng đã học
            var userProgress = await _context.UserCourseProgress
                .Where(p => p.UserId == userId)
                .Include(p => p.Course)
                .ToListAsync();

            if (!userProgress.Any())
            {
                // Nếu người dùng chưa học khóa nào, trả về các khóa học phổ biến
                return await GetPopularCourses(limit);
            }

            // Lấy danh sách ID khóa học đã học
            var completedCourseIds = userProgress.Select(p => p.CourseId).ToList();

            // Lấy danh sách khóa học chưa học
            var availableCourses = await _context.courses
                .Where(c => !completedCourseIds.Contains(c.Id))
                .ToListAsync();

            // Tính điểm liên quan cho mỗi khóa học
            var recommendations = new List<CourseRecommendation>();
            foreach (var course in availableCourses)
            {
                var relevanceScore = CalculateRelevanceScore(course, userProgress);
                var reason = GenerateRecommendationReason(course, userProgress, relevanceScore);

                recommendations.Add(new CourseRecommendation
                {
                    CourseId = course.Id,
                    CourseName = course.Name,
                    Description = course.Description,
                    Price = course.Price,
                    RelevanceScore = relevanceScore,
                    RecommendationReason = reason
                });
            }

            // Sắp xếp theo điểm liên quan và trả về top N khóa học
            return recommendations
                .OrderByDescending(r => r.RelevanceScore)
                .Take(limit);
        }

        private async Task<IEnumerable<CourseRecommendation>> GetPopularCourses(int limit)
        {
            // Lấy các khóa học có nhiều người học nhất
            var popularCourses = await _context.UserCourseProgress
                .GroupBy(p => p.CourseId)
                .Select(g => new
                {
                    CourseId = g.Key,
                    StudentCount = g.Count()
                })
                .OrderByDescending(x => x.StudentCount)
                .Take(limit)
                .ToListAsync();

            var courseIds = popularCourses.Select(p => p.CourseId).ToList();
            var courses = await _context.courses
                .Where(c => courseIds.Contains(c.Id))
                .ToListAsync();

            return courses.Select(c => new CourseRecommendation
            {
                CourseId = c.Id,
                CourseName = c.Name,
                Description = c.Description,
                Price = c.Price,
                RelevanceScore = 1.0,
                RecommendationReason = "Khóa học phổ biến"
            });
        }

        private double CalculateRelevanceScore(courses course, List<UserCourseProgress> userProgress)
        {
            double score = 0;

            // Tính điểm dựa trên tiến độ học tập của người dùng
            var completedCourses = userProgress.Where(p => p.CompletedVideos == p.TotalVideos).ToList();
            var inProgressCourses = userProgress.Where(p => p.CompletedVideos > 0 && p.CompletedVideos < p.TotalVideos).ToList();

            // Ưu tiên các khóa học cùng loại với khóa học đã hoàn thành
            if (completedCourses.Any())
            {
                score += 0.4;
            }

            // Ưu tiên các khóa học cùng loại với khóa học đang học
            if (inProgressCourses.Any())
            {
                score += 0.3;
            }

            // Thêm điểm ngẫu nhiên để đa dạng gợi ý
            var random = new Random();
            score += random.NextDouble() * 0.3;

            return score;
        }

        private string GenerateRecommendationReason(courses course, List<UserCourseProgress> userProgress, double relevanceScore)
        {
            var completedCourses = userProgress.Where(p => p.CompletedVideos == p.TotalVideos).ToList();
            var inProgressCourses = userProgress.Where(p => p.CompletedVideos > 0 && p.CompletedVideos < p.TotalVideos).ToList();

            if (completedCourses.Any())
            {
                return "Dựa trên các khóa học bạn đã hoàn thành";
            }
            else if (inProgressCourses.Any())
            {
                return "Dựa trên các khóa học bạn đang học";
            }
            else
            {
                return "Khóa học phổ biến";
            }
        }

        public async Task<IEnumerable<TopRatedCourse>> GetTopRatedCoursesAsync(int limit = 10)
        {
            // Lấy thông tin đánh giá và số học viên cho mỗi khóa học
            var courseStats = await _context.Ratings
                .GroupBy(r => r.CourseId)
                .Select(g => new
                {
                    CourseId = g.Key,
                    AverageRating = g.Average(r => r.RatingValue),
                    TotalRatings = g.Count()
                })
                .ToListAsync();

            // Lấy số học viên cho mỗi khóa học
            var studentCounts = await _context.UserCourseProgress
                .GroupBy(p => p.CourseId)
                .Select(g => new
                {
                    CourseId = g.Key,
                    TotalStudents = g.Count()
                })
                .ToListAsync();

            // Lấy thông tin chi tiết của các khóa học
            var courses = await _context.courses
                .Where(c => courseStats.Select(s => s.CourseId).Contains(c.Id))
                .ToListAsync();

            // Kết hợp thông tin và tính toán điểm xếp hạng
            var topRatedCourses = courses
                .Select(c => new TopRatedCourse
                {
                    CourseId = c.Id,
                    CourseName = c.Name,
                    Description = c.Description,
                    Price = c.Price,
                    AverageRating = courseStats.FirstOrDefault(s => s.CourseId == c.Id)?.AverageRating ?? 0,
                    TotalRatings = courseStats.FirstOrDefault(s => s.CourseId == c.Id)?.TotalRatings ?? 0,
                    TotalStudents = studentCounts.FirstOrDefault(s => s.CourseId == c.Id)?.TotalStudents ?? 0
                })
                .OrderByDescending(c => c.AverageRating)
                .ThenByDescending(c => c.TotalRatings)
                .Take(limit)
                .ToList();

            return topRatedCourses;
        }

        public async Task AddRelatedCourseAsync(int courseId, int relatedCourseId)
        {
            var exists = await _context.RelatedCourses
                .AnyAsync(rc => rc.CourseId == courseId && rc.RelatedCourseId == relatedCourseId);
            if (!exists)
            {
                _context.RelatedCourses.Add(new RelatedCourse
                {
                    CourseId = courseId,
                    RelatedCourseId = relatedCourseId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<courses>> GetRelatedCoursesAsync(int courseId)
        {
            var relatedIds = await _context.RelatedCourses
                .Where(rc => rc.CourseId == courseId)
                .Select(rc => rc.RelatedCourseId)
                .ToListAsync();
            return await _context.courses
                .Where(c => relatedIds.Contains(c.Id))
                .ToListAsync();
        }

        public async Task UpdateCourseStatusAsync(int courseId, CourseStatus status)
        {
            var course = await _context.courses.FindAsync(courseId);
            if (course != null)
            {
                course.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<courses>> SearchCoursesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _context.courses.ToListAsync();
            }

            query = query.ToLower().Trim();
            return await _context.courses
                .Where(c => c.Status == CourseStatus.Approved && // Only search approved courses
                    (c.Name.ToLower().Contains(query) || 
                     (c.Description != null && c.Description.ToLower().Contains(query))))
                .ToListAsync();
        }

        public async Task<List<CourseVideoOverviewDto>> GetCourseOverviewAsync(int courseId)
        {
            var videos = await _context.Videos.Where(v => v.CourseId == courseId).ToListAsync();
            var videoIds = videos.Select(v => v.Id).ToList();
            var progresses = await _context.VideoProgresses
                .Where(p => videoIds.Contains(p.VideoId) && p.ProgressPercentage > 0)
                .ToListAsync();
            var result = videos.Select(v => new CourseVideoOverviewDto
            {
                VideoId = v.Id,
                Title = v.Title,
                ViewCount = v.ViewCount,
                LearnedCount = progresses.Count(p => p.VideoId == v.Id)
            }).ToList();
            return result;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            // Tổng số người dùng
            var totalUsers = await _context.Users.CountAsync();

            // Tổng số khóa học
            var totalCourses = await _context.courses.CountAsync();

            // Số học viên đang học (có tiến độ học tập trong 30 ngày gần đây)
            var thirtyDaysAgo = now.AddDays(-30);
            var activeStudents = await _context.UserCourseProgress
                .Where(p => p.LastAccessed >= thirtyDaysAgo)
                .Select(p => p.UserId)
                .Distinct()
                .CountAsync();

            // Top khóa học đăng ký nhiều nhất
            var enrollmentGroups = await _context.Enrollments
                .GroupBy(e => e.CourseId)
                .Select(g => new
                {
                    CourseId = g.Key,
                    EnrollmentCount = g.Count()
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .Take(5)
                .ToListAsync();

            var topEnrolledCourses = new List<TopEnrolledCourseDto>();
            foreach (var group in enrollmentGroups)
            {
                var course = await _context.courses.FindAsync(group.CourseId);
                if (course != null)
                {
                    var averageRating = await _context.Ratings
                        .Where(r => r.CourseId == group.CourseId)
                        .AverageAsync(r => (double?)r.RatingValue) ?? 0;

                    topEnrolledCourses.Add(new TopEnrolledCourseDto
                    {
                        CourseId = course.Id,
                        CourseName = course.Name,
                        ImageUrl = course.ImageUrl,
                        EnrollmentCount = group.EnrollmentCount,
                        AverageRating = averageRating
                    });
                }
            }

            // Phân bố khóa học theo danh mục
            var categories = await _context.Categories.ToListAsync();
            var categoryDistribution = new List<CategoryDistributionDto>();
            
            foreach (var category in categories)
            {
                var courseCount = await _context.courses
                    .CountAsync(c => c.CategoryId == category.Id);

                categoryDistribution.Add(new CategoryDistributionDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    CourseCount = courseCount
                });
            }

            // Số người dùng mới trong tháng
            var newUsersThisMonth = await _context.Users
                .CountAsync(u => u.CreatedAt >= firstDayOfMonth);

            return new DashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalCourses = totalCourses,
                ActiveStudents = activeStudents,
                TopEnrolledCourses = topEnrolledCourses,
                CategoryDistribution = categoryDistribution,
                NewUsersThisMonth = newUsersThisMonth
            };
        }
    }
}