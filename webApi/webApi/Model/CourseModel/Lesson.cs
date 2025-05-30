using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webApi.Model.CourseModel
{
    public enum LessonType
    {
        Video = 0,
        Article = 1,
        Mixed = 2
    }

    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public LessonType Type { get; set; }

        public int SectionId { get; set; }
        [ForeignKey("SectionId")]
        public Section Section { get; set; }

        public string Content { get; set; }
        public string Duration { get; set; }
    }
} 