using System.Collections.Generic;

namespace webApi.Model.CourseModel
{
    public class LessonUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public int SectionId { get; set; }
        public string Content { get; set; }
        public string Duration { get; set; }
    }

    public class SectionUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }
        public List<LessonUpdateDto> Lessons { get; set; }
    }

    public class CourseUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public string VideoDemoUrl { get; set; }
        public int Status { get; set; }
        public int Level { get; set; }
        public int CategoryId { get; set; }
        public string InstructorId { get; set; }
        public List<SectionUpdateDto> Sections { get; set; }
    }
} 