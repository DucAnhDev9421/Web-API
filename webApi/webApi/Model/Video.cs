using System.ComponentModel.DataAnnotations;

namespace webApi.Model
{
    public class Video
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string VideoUrl { get; set; }

        public int CourseId { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; }
    }
}
