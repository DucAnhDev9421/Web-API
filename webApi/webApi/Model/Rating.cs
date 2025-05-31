using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.CourseModel;
using webApi.Model.UserModel;

namespace webApi.Model
{
    public class Rating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column("CourseId")]
        public int CourseId { get; set; }

        [Required]
        [Range(1, 5)]
        public int RatingValue { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("UserId")]
        public UserInfo User { get; set; }

        [ForeignKey("CourseId")]
        public courses Course { get; set; }
    }
} 