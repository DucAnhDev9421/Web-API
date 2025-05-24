using System.Collections.Generic;

namespace webApi.Model.CourseModel
{
    public class LessonCreateDto
    {
        public string Title { get; set; }
        public int Type { get; set; } // 0: Video, 1: Article, 2: Mixed
        public string Content { get; set; }
    }

    public class SectionCreateDto
    {
        public string Title { get; set; }
        public List<LessonCreateDto> Lessons { get; set; }
    }

    public class CourseCreateDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int Status { get; set; }
        public int Level { get; set; }
        public int? CategoryId { get; set; }
        public List<SectionCreateDto> Sections { get; set; }
    }
} 