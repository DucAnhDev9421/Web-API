using System.ComponentModel.DataAnnotations.Schema;

namespace webApi.Model.CourseModel
{
    public class RelatedCourse
    {
        public int CourseId { get; set; }
        public courses Course { get; set; }

        public int RelatedCourseId { get; set; }
        public courses Related { get; set; }
    }
} 