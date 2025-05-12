using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.CourseModel;
using webApi.Model.UserModel;

namespace webApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserInfo> GetUserByIdAsync(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<UserInfo> CreateOrUpdateUserAsync(UserInfo user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            
            if (existingUser == null)
            {
                user.CreatedAt = DateTime.UtcNow;
                _context.Users.Add(user);
            }
            else
            {
                existingUser.Email = user.Email;
                existingUser.Username = user.Username;
                existingUser.FirstName = user.FirstName;
                existingUser.LastName = user.LastName;
                existingUser.ImageUrl = user.ImageUrl;
                existingUser.ProfileImageUrl = user.ProfileImageUrl;
                existingUser.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(string id)
        {
            // Lấy thông tin người dùng
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            // Lấy thông tin tiến độ học tập
            var userProgress = await _context.UserCourseProgress
                .Where(p => p.UserId == id)
                .ToListAsync();

            var totalCourses = userProgress.Count;
            var completedCourses = userProgress.Count(p => p.CompletedVideos == p.TotalVideos);
            var inProgressCourses = userProgress.Count(p => p.CompletedVideos > 0 && p.CompletedVideos < p.TotalVideos);
            
            double averageProgress = 0;
            if (totalCourses > 0)
            {
                averageProgress = userProgress
                    .Average(p => p.TotalVideos > 0 ? (double)p.CompletedVideos / p.TotalVideos : 0) * 100;
            }

            DateTime? lastActivityDate = null;
            if (userProgress.Any())
            {
                lastActivityDate = userProgress.Max(p => p.LastAccessed);
            }

            // Đếm số khóa học yêu thích
            var favoriteCourses = await _context.UserFavoriteCourses
                .CountAsync(f => f.UserId == id);

            // Tạo đối tượng profile
            var profile = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ImageUrl = user.ImageUrl,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Stats = new UserStatsDto
                {
                    TotalCourses = totalCourses,
                    CompletedCourses = completedCourses,
                    InProgressCourses = inProgressCourses,
                    FavoriteCourses = favoriteCourses,
                    AverageProgress = Math.Round(averageProgress, 2),
                    LastActivityDate = lastActivityDate
                }
            };

            return profile;
        }

        public async Task<List<courses>> GetUserFavoriteCoursesAsync(string userId)
        {
            return await _context.UserFavoriteCourses
                .Where(f => f.UserId == userId)
                .Include(f => f.Course)
                .Select(f => f.Course)
                .ToListAsync();
        }

        public async Task<bool> ToggleFavoriteCourseAsync(string userId, int courseId)
        {
            // Check if the course exists
            var course = await _context.courses.FindAsync(courseId);
            if (course == null)
                return false;

            // Check if the user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            // Check if the favorite already exists
            var favorite = await _context.UserFavoriteCourses
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId);

            if (favorite != null)
            {
                // If exists, remove it (toggle off)
                _context.UserFavoriteCourses.Remove(favorite);
                await _context.SaveChangesAsync();
                return false; // Indicates it's now not a favorite
            }
            else
            {
                // If doesn't exist, add it (toggle on)
                var newFavorite = new UserFavoriteCourse
                {
                    UserId = userId,
                    CourseId = courseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserFavoriteCourses.Add(newFavorite);
                await _context.SaveChangesAsync();
                return true; // Indicates it's now a favorite
            }
        }

        public async Task<bool> IsFavoriteCourseAsync(string userId, int courseId)
        {
            return await _context.UserFavoriteCourses
                .AnyAsync(f => f.UserId == userId && f.CourseId == courseId);
        }
    }
} 