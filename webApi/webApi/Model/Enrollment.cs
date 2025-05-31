using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.CourseModel;
using webApi.Model.UserModel;

namespace webApi.Model
{
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column("CourseId")]
        public int CourseId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public UserInfo User { get; set; }

        [ForeignKey("CourseId")]
        public courses Course { get; set; }
    }
} 