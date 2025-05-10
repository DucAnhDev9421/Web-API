namespace webApi.Model.CourseModel
{
    public class TopRatedCourse
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalStudents { get; set; }
    }
} 