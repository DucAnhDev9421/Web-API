using webApi.Model;

namespace webApi.Repositories
{
    public interface IUserRepository
    {
        Task<UserInfo> GetUserByIdAsync(string id);
        Task<UserInfo> CreateOrUpdateUserAsync(UserInfo user);
        Task<bool> DeleteUserAsync(string id);
    }
} 