using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminOverview> GetOverviewAsync()
        {
            var now = DateTime.UtcNow;
            var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

            // Course Statistics
            var courses = await _context.courses.ToListAsync();
            var videos = await _context.Videos.ToListAsync();
            var courseStats = new CourseStats
            {
                TotalCourses = courses.Count,
                FreeCourses = courses.Count(c => c.Price == 0),
                PaidCourses = courses.Count(c => c.Price > 0),
                TotalVideos = videos.Count,
                AverageVideosPerCourse = courses.Count > 0 ? (double)videos.Count / courses.Count : 0
            };

            // User Statistics
            var users = await _context.Users.ToListAsync();
            var userProgress = await _context.UserCourseProgress.ToListAsync();
            var userStats = new UserStats
            {
                TotalUsers = users.Count,
                ActiveUsers = userProgress.Select(p => p.UserId).Distinct().Count(),
                NewUsersThisMonth = users.Count(u => u.CreatedAt >= firstDayOfMonth),
                UsersWithProgress = userProgress.Select(p => p.UserId).Distinct().Count()
            };

            // Rating Statistics
            var ratings = await _context.Ratings.ToListAsync();
            var ratingDistribution = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingDistribution[i] = ratings.Count(r => r.RatingValue == i);
            }

            var ratingStats = new RatingStats
            {
                TotalRatings = ratings.Count,
                AverageRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0,
                RatingDistribution = ratingDistribution
            };

            return new AdminOverview
            {
                CourseStats = courseStats,
                UserStats = userStats,
                RatingStats = ratingStats
            };
        }
    }
} 