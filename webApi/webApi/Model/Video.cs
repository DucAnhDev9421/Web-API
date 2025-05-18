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

        public string Thumbnail { get; set; }

        [Range(0, 86400)] // Tối đa 24 giờ (24 * 60 * 60 giây)
        public int Duration { get; set; } // Thời lượng video (giây)

        [Range(0, 1000000000)] // Tối đa 1 tỷ lượt xem
        public int ViewCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CourseId { get; set; }
        public int Order { get; set; }
        public bool IsVisible { get; set; }
    }
}
