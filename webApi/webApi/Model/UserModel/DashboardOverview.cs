namespace webApi.Model.UserModel
{
    public class DashboardOverview
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int NotStartedCourses { get; set; }
        public List<CourseProgress> RecentCourses { get; set; }
    }

    public class CourseProgress
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public double Progress { get; set; }
        public DateTime LastAccessed { get; set; }
    }
} 