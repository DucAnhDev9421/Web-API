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
            try
            {
                // Validate required fields
                if (string.IsNullOrEmpty(user.Id))
                    throw new ArgumentException("User ID is required");
                if (string.IsNullOrEmpty(user.Email))
                    throw new ArgumentException("Email is required");

                var existingUser = await _context.Users.FindAsync(user.Id);
                
                if (existingUser == null)
                {
                    // Tạo mới user
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                    
                    // Đảm bảo các trường không null
                    user.Username ??= user.Id;
                    user.FirstName ??= string.Empty;
                    user.LastName ??= string.Empty;
                    user.ImageUrl ??= string.Empty;
                    user.ProfileImageUrl ??= string.Empty;
                    user.Role ??= "user";
                    user.JobTitle ??= string.Empty;
                    user.Bio ??= string.Empty;

                    _context.Users.Add(user);
                }
                else
                {
                    // Cập nhật thông tin user
                    existingUser.Email = user.Email;
                    existingUser.Username = user.Username ?? existingUser.Username;
                    existingUser.FirstName = user.FirstName ?? existingUser.FirstName;
                    existingUser.LastName = user.LastName ?? existingUser.LastName;
                    existingUser.ImageUrl = user.ImageUrl ?? existingUser.ImageUrl;
                    existingUser.ProfileImageUrl = user.ProfileImageUrl ?? existingUser.ProfileImageUrl;
                    existingUser.Role = user.Role ?? existingUser.Role;
                    existingUser.JobTitle = user.JobTitle ?? existingUser.JobTitle;
                    existingUser.Bio = user.Bio ?? existingUser.Bio;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    // Cập nhật entity
                    _context.Entry(existingUser).State = EntityState.Modified;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Log chi tiết lỗi database
                    Console.WriteLine($"Database Error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Database Error: {dbEx.InnerException.Message}");
                    }
                    throw new Exception("Không thể lưu thông tin người dùng vào database", dbEx);
                }

                return existingUser ?? user;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Console.WriteLine($"Lỗi khi lưu user: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        Console.WriteLine($"Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                throw;
            }
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
                Role = user.Role,
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

        public async Task<List<FavoriteCourseDetailDto>> GetUserFavoriteCoursesAsync(string userId)
        {
            var favoriteCourses = await _context.UserFavoriteCourses
                .Where(f => f.UserId == userId)
                .Include(f => f.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(f => f.Course)
                    .ThenInclude(c => c.Ratings)
                .Include(f => f.Course)
                    .ThenInclude(c => c.Enrollments)
                .Select(f => new FavoriteCourseDetailDto
                {
                    Id = f.Course.Id,
                    Name = f.Course.Name,
                    Description = f.Course.Description,
                    ImageUrl = f.Course.ImageUrl,
                    Price = f.Course.Price,
                    InstructorName = f.Course.Instructor.FirstName + " " + f.Course.Instructor.LastName,
                    InstructorImageUrl = f.Course.Instructor.ImageUrl,
                    AverageRating = f.Course.Ratings.Any() 
                        ? Math.Round(f.Course.Ratings.Average(r => r.RatingValue), 1)
                        : 0,
                    EnrollmentCount = f.Course.Enrollments.Count,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            return favoriteCourses;
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

        public async Task<List<UserCourseProgressDto>> GetAllCourseProgressAsync(string userId)
        {
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            var progresses = await _context.UserCourseProgress
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var result = enrollments.Select(e => {
                var progress = progresses.FirstOrDefault(p => p.CourseId == e.CourseId);
                double percent = (progress != null && progress.TotalVideos > 0)
                    ? (double)progress.CompletedVideos / progress.TotalVideos * 100
                    : 0;
                string status = percent >= 100 ? "completed" : (percent <= 0 ? "not_started" : "in_progress");
                return new UserCourseProgressDto
                {
                    CourseId = e.CourseId,
                    Name = e.Course?.Name,
                    ProgressPercentage = percent,
                    LastActivityAt = progress?.LastAccessed,
                    CompletionStatus = status
                };
            }).ToList();
            return result;
        }
    }
} 