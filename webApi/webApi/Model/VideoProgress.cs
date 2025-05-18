using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.UserModel;

namespace webApi.Model
{
    public class VideoProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int VideoId { get; set; }

        [Range(0, 100)]
        public int ProgressPercentage { get; set; } // Tiến độ xem video (%)

        public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public UserInfo User { get; set; }

        [ForeignKey("VideoId")]
        public Video Video { get; set; }
    }
} 