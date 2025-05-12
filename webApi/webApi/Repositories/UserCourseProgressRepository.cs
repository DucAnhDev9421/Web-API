using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.UserModel;
using webApi.Model.CategoryModel;

namespace webApi.Repositories
{
    public class UserCourseProgressRepository : IUserCourseProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public UserCourseProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserCourseProgress> GetUserCourseProgressAsync(string userId, int courseId)
        {
            return await _context.UserCourseProgress
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CourseId == courseId);
        }

        public async Task<IEnumerable<UserCourseProgress>> GetUserProgressAsync(string userId)
        {
            return await _context.UserCourseProgress
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserCourseProgress> CreateOrUpdateProgressAsync(UserCourseProgress progress)
        {
            var existingProgress = await _context.UserCourseProgress
                .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.CourseId == progress.CourseId);

            if (existingProgress == null)
            {
                progress.CreatedAt = DateTime.UtcNow;
                _context.UserCourseProgress.Add(progress);
            }
            else
            {
                existingProgress.CompletedVideos = progress.CompletedVideos;
                existingProgress.TotalVideos = progress.TotalVideos;
                existingProgress.LastAccessed = DateTime.UtcNow;
                existingProgress.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return progress;
        }

        public async Task<bool> DeleteProgressAsync(int id)
        {
            var progress = await _context.UserCourseProgress.FindAsync(id);
            if (progress == null)
                return false;

            _context.UserCourseProgress.Remove(progress);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DashboardOverview> GetUserDashboardAsync(string userId)
        {
            var userProgress = await _context.UserCourseProgress
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var totalCourses = userProgress.Count;
            var completedCourses = userProgress.Count(p => p.CompletedVideos == p.TotalVideos);
            var inProgressCourses = userProgress.Count(p => p.CompletedVideos > 0 && p.CompletedVideos < p.TotalVideos);
            var notStartedCourses = userProgress.Count(p => p.CompletedVideos == 0);

            var recentCourses = userProgress
                .OrderByDescending(p => p.LastAccessed)
                .Take(5)
                .Select(p => new CourseProgress
                {
                    CourseId = p.CourseId,
                    CourseName = p.Course.Name,
                    Progress = p.TotalVideos > 0 ? (double)p.CompletedVideos / p.TotalVideos * 100 : 0,
                    LastAccessed = p.LastAccessed
                })
                .ToList();

            return new DashboardOverview
            {
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                NotStartedCourses = notStartedCourses,
                RecentCourses = recentCourses
            };
        }

        public async Task<UserStatistics> GetUserStatisticsAsync(string userId)
        {
            // Lấy tiến độ học tập của người dùng
            var userProgress = await _context.UserCourseProgress
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // Lấy danh sách khóa học yêu thích
            var favoriteCourses = await _context.UserFavoriteCourses
                .Where(f => f.UserId == userId)
                .Select(f => f.CourseId)
                .ToListAsync();

            // Tính toán các thống kê cơ bản
            var totalCourses = userProgress.Count;
            var completedCourses = userProgress.Count(p => p.CompletedVideos == p.TotalVideos);
            var inProgressCourses = userProgress.Count(p => p.CompletedVideos > 0 && p.CompletedVideos < p.TotalVideos);
            var notStartedCourses = userProgress.Count(p => p.CompletedVideos == 0);
            
            // Tính toán tiến độ trung bình
            double averageProgress = 0;
            if (totalCourses > 0)
            {
                averageProgress = userProgress
                    .Average(p => p.TotalVideos > 0 ? (double)p.CompletedVideos / p.TotalVideos : 0) * 100;
            }

            // Lấy ngày hoạt động gần nhất
            DateTime? lastActivityDate = null;
            if (userProgress.Any())
            {
                lastActivityDate = userProgress.Max(p => p.LastAccessed);
            }

            // Lấy danh sách khóa học gần đây
            var recentCourses = userProgress
                .OrderByDescending(p => p.LastAccessed)
                .Take(5)
                .Select(p => new CourseStatistics
                {
                    CourseId = p.CourseId,
                    CourseName = p.Course.Name,
                    Progress = p.TotalVideos > 0 ? (double)p.CompletedVideos / p.TotalVideos * 100 : 0,
                    LastAccessed = p.LastAccessed,
                    IsFavorite = favoriteCourses.Contains(p.CourseId),
                    CompletedVideos = p.CompletedVideos,
                    TotalVideos = p.TotalVideos
                })
                .ToList();

            // Lấy thống kê theo danh mục (nếu có)
            var categoryBreakdown = new List<CategoryStatistics>();
            
            // Lấy danh sách các danh mục và khóa học liên quan
            var coursesWithCategories = await _context.courses
                .Join(_context.UserCourseProgress.Where(p => p.UserId == userId),
                    course => course.Id,
                    progress => progress.CourseId,
                    (course, progress) => new { Course = course, Progress = progress })
                .ToListAsync();

            // Nhóm theo danh mục và tính toán thống kê
            if (coursesWithCategories.Any())
            {
                var categoryGroups = coursesWithCategories
                    .GroupBy(c => new { CategoryId = 1, CategoryName = "All" }) // Thay thế bằng thông tin danh mục thực tế nếu có
                    .Select(g => new CategoryStatistics
                    {
                        CategoryId = g.Key.CategoryId,
                        CategoryName = g.Key.CategoryName,
                        CourseCount = g.Count(),
                        CompletedCourses = g.Count(c => c.Progress.CompletedVideos == c.Progress.TotalVideos),
                        AverageProgress = g.Average(c => c.Progress.TotalVideos > 0 
                            ? (double)c.Progress.CompletedVideos / c.Progress.TotalVideos * 100 
                            : 0)
                    })
                    .ToList();
                
                categoryBreakdown.AddRange(categoryGroups);
            }

            // Lấy thống kê hoạt động theo ngày (7 ngày gần nhất)
            var activityByDay = new List<ActivityByDay>();
            var startDate = DateTime.UtcNow.Date.AddDays(-6);
            
            for (int i = 0; i < 7; i++)
            {
                var currentDate = startDate.AddDays(i);
                var nextDate = currentDate.AddDays(1);
                
                // Đếm số video đã hoàn thành trong ngày
                // Lưu ý: Đây là mô phỏng, cần có bảng ghi lại hoạt động hàng ngày để có dữ liệu chính xác
                var completedVideosForDay = 0;
                var activeMinutes = 0;
                
                // Thêm vào danh sách
                activityByDay.Add(new ActivityByDay
                {
                    Date = currentDate,
                    CompletedVideos = completedVideosForDay,
                    ActiveMinutes = activeMinutes
                });
            }

            // Tạo và trả về đối tượng thống kê
            return new UserStatistics
            {
                TotalCourses = totalCourses,
                CompletedCourses = completedCourses,
                InProgressCourses = inProgressCourses,
                NotStartedCourses = notStartedCourses,
                FavoriteCourses = favoriteCourses.Count,
                AverageProgress = Math.Round(averageProgress, 2),
                LastActivityDate = lastActivityDate,
                RecentCourses = recentCourses,
                CategoryBreakdown = categoryBreakdown,
                ActivityByDay = activityByDay
            };
        }
    }
} 