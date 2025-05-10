using webApi.Model.CourseModel;

namespace webApi.Model.UserModel
{
    public class EnrollmentDetailDto
    {
        public int Id { get; set; }
        public CourseInfoDto Course { get; set; }
        public ProgressInfoDto Progress { get; set; }
        public InstructorInfoDto Instructor { get; set; }
        public DateTime EnrolledAt { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    public class CourseInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
    }

    public class ProgressInfoDto
    {
        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
        public double ProgressPercentage => TotalVideos > 0 ? (double)CompletedVideos / TotalVideos * 100 : 0;
        public string Status => ProgressPercentage switch
        {
            0 => "Not Started",
            100 => "Completed",
            _ => "In Progress"
        };
    }

    public class InstructorInfoDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }
    }
} 