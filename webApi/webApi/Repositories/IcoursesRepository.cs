using webApi.Model.CourseModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webApi.Repositories
{
    public class CourseVideoOverviewDto
    {
        public int VideoId { get; set; }
        public string Title { get; set; }
        public int ViewCount { get; set; }
        public int LearnedCount { get; set; }
    }

    public class CourseWithCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusText { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelText { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public interface IcoursesRepository
    {
        Task<IEnumerable<CourseWithCategoryDto>> GetcoursesAsync();
        Task<CourseWithCategoryDto> GetcoursesByIdAsync(int id);
        Task AddcoursesAsync(courses courses);
        Task UpdatecoursesAsync(courses courses);
        Task DeletecoursesAsync(int id);
        Task<IEnumerable<courses>> GetFreecoursesAsync();
        Task<IEnumerable<CourseRecommendation>> GetRecommendedCoursesAsync(string userId, int limit = 5);
        Task<IEnumerable<TopRatedCourse>> GetTopRatedCoursesAsync(int limit = 10);
        Task AddRelatedCourseAsync(int courseId, int relatedCourseId);
        Task<IEnumerable<courses>> GetRelatedCoursesAsync(int courseId);
        Task UpdateCourseStatusAsync(int courseId, CourseStatus status);
        Task<IEnumerable<courses>> SearchCoursesAsync(string query);
        Task<List<CourseVideoOverviewDto>> GetCourseOverviewAsync(int courseId);
    }
}
