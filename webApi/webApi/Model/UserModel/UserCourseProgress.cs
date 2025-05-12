using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.CourseModel;

namespace webApi.Model.UserModel
{
    public class UserCourseProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int CompletedVideos { get; set; }
        public int TotalVideos { get; set; }
        public DateTime LastAccessed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public UserInfo User { get; set; }

        [ForeignKey("CourseId")]
        public courses Course { get; set; }
    }
} 