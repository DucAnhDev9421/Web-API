namespace webApi.Model
{
    public class AdminOverview
    {
        public CourseStats CourseStats { get; set; }
        public UserStats UserStats { get; set; }
        public RatingStats RatingStats { get; set; }
    }

    public class CourseStats
    {
        public int TotalCourses { get; set; }
        public int FreeCourses { get; set; }
        public int PaidCourses { get; set; }
        public int TotalVideos { get; set; }
        public double AverageVideosPerCourse { get; set; }
    }

    public class UserStats
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int UsersWithProgress { get; set; }
    }

    public class RatingStats
    {
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } // Key: rating (1-5), Value: count
    }
} 