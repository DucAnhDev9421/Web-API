using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webApi.Model
{
    public class Note
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Content { get; set; }
        public int VideoId { get; set; } 

        [ForeignKey("VideoId")]        
        public Video Video { get; set; }
    }
}
