using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.CourseModel;

namespace webApi.Model.UserModel
{
    public class UserFavoriteCourse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public UserInfo User { get; set; }

        [ForeignKey("CourseId")]
        public courses Course { get; set; }
    }
} 