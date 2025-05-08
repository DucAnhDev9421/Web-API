using webApi.Model;

namespace webApi.Repositories
{
    public interface IcoursesRepository
    {
        Task<IEnumerable<courses>> GetcoursesAsync();
        Task<courses> GetcoursesByIdAsync(int id);
        Task AddcoursesAsync(courses courses);
        Task UpdatecoursesAsync(courses courses);
        Task DeletecoursesAsync(int id);
        Task<IEnumerable<courses>> GetFreecoursesAsync();
    }
}
