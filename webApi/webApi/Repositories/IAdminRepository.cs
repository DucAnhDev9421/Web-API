using webApi.Model;

namespace webApi.Repositories
{
    public interface IAdminRepository
    {
        Task<AdminOverview> GetOverviewAsync();
    }
} 