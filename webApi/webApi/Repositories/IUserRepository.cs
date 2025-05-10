using webApi.Model.CourseModel;
using webApi.Model.UserModel;

namespace webApi.Repositories
{
    public interface IUserRepository
    {
        Task<UserInfo> GetUserByIdAsync(string id);
        Task<UserInfo> CreateOrUpdateUserAsync(UserInfo user);
        Task<bool> DeleteUserAsync(string id);
        Task<UserProfileDto> GetUserProfileAsync(string id);
        Task<List<courses>> GetUserFavoriteCoursesAsync(string userId);
        Task<bool> ToggleFavoriteCourseAsync(string userId, int courseId);
        Task<bool> IsFavoriteCourseAsync(string userId, int courseId);
    }
} 