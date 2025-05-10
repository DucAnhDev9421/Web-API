using webApi.Model.CourseModel;

namespace webApi.Model.UserModel
{
    public class UserStatistics
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int NotStartedCourses { get; set; }
        public int FavoriteCourses { get; set; }
        public double AverageProgress { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public List<CourseStatistics> RecentCourses { get; set; }
        public List<CategoryStatistics> CategoryBreakdown { get; set; }
        public List<ActivityByDay> ActivityByDay { get; set; }
    }

    public class CourseStatistics
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public double Progress { get; set; }
        public DateTime LastAccessed { get; set; }
        public bool IsFavorite { get; set; }
        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
    }

    public class CategoryStatistics
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int CourseCount { get; set; }
        public int CompletedCourses { get; set; }
        public double AverageProgress { get; set; }
    }

    public class ActivityByDay
    {
        public DateTime Date { get; set; }
        public int CompletedVideos { get; set; }
        public int ActiveMinutes { get; set; }
    }
} 