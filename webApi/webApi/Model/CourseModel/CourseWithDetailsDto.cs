using System;

namespace webApi.Model.CourseModel
{
    public class CourseWithDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string VideoDemoUrl { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusText { get; set; }
        public CourseLevel Level { get; set; }
        public string LevelText { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public InstructorInfo Instructor { get; set; }
        public string TotalDuration { get; set; }
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
    }
} 