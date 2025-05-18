using System.Threading.Tasks;
using webApi.Model;

namespace webApi.Repositories
{
    public interface IEnrollmentRepository
    {
        Task<bool> EnrollAsync(string userId, int courseId);
        Task<List<Enrollment>> GetEnrollmentsByUserAsync(string userId);
        Task<bool> UnenrollAsync(string userId, int courseId);
    }
} 