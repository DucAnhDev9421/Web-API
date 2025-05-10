using webApi.Model.CourseModel;

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
        Task<IEnumerable<CourseRecommendation>> GetRecommendedCoursesAsync(string userId, int limit = 5);
        Task<IEnumerable<TopRatedCourse>> GetTopRatedCoursesAsync(int limit = 10);
    }
}
