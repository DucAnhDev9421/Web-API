using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webApi.Model.CourseModel
{
    public class Section
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public int CourseId { get; set; }
        [ForeignKey("CourseId")]
        public courses Course { get; set; }

        public ICollection<Lesson> Lessons { get; set; }
    }
} 