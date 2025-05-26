using System.Collections.Generic;

namespace webApi.Model.CourseModel
{
    public class LessonResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
    }

    public class SectionResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public List<LessonResponseDto> Lessons { get; set; }
    }

    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string VideoDemoUrl { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int Level { get; set; }
        public string LevelText { get; set; }
        public int? CategoryId { get; set; }
        public List<SectionResponseDto> Sections { get; set; }
    }
} 