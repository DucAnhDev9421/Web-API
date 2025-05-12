namespace webApi.Model.CourseModel
{
    public class CourseRecommendation
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public double RelevanceScore { get; set; }
        public string RecommendationReason { get; set; }
    }
} 