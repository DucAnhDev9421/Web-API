using webApi.Model.CourseModel;
using webApi.Model.UserModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webApi.Repositories
{
    public interface IUserRepository
    {
        Task<UserInfo> GetUserByIdAsync(string id);
        Task<UserInfo> CreateOrUpdateUserAsync(UserInfo user);
        Task<bool> DeleteUserAsync(string id);
        Task<UserProfileDto> GetUserProfileAsync(string id);
        Task<List<FavoriteCourseDetailDto>> GetUserFavoriteCoursesAsync(string userId);
        Task<bool> ToggleFavoriteCourseAsync(string userId, int courseId);
        Task<bool> IsFavoriteCourseAsync(string userId, int courseId);
        Task<List<UserCourseProgressDto>> GetAllCourseProgressAsync(string userId);
    }

    public class FavoriteCourseDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public string InstructorName { get; set; }
        public string InstructorImageUrl { get; set; }
        public double AverageRating { get; set; }
        public int EnrollmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserCourseProgressDto
    {
        public int CourseId { get; set; }
        public string Name { get; set; }
        public double ProgressPercentage { get; set; }
        public System.DateTime? LastActivityAt { get; set; }
        public string CompletionStatus { get; set; }
    }
} 