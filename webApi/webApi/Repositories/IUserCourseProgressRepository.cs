using webApi.Model.UserModel;

namespace webApi.Repositories
{
    public interface IUserCourseProgressRepository
    {
        Task<UserCourseProgress> GetUserCourseProgressAsync(string userId, int courseId);
        Task<IEnumerable<UserCourseProgress>> GetUserProgressAsync(string userId);
        Task<UserCourseProgress> CreateOrUpdateProgressAsync(UserCourseProgress progress);
        Task<bool> DeleteProgressAsync(int id);
        Task<DashboardOverview> GetUserDashboardAsync(string userId);
        Task<UserStatistics> GetUserStatisticsAsync(string userId);
    }
} 