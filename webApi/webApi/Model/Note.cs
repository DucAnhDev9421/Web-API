using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.UserModel;

namespace webApi.Model
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }
        
        [Required]
        public int VideoId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("VideoId")]        
        public Video Video { get; set; }
        
        [ForeignKey("UserId")]
        public UserInfo User { get; set; }
    }
}
