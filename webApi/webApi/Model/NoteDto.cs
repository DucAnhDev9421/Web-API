using System.ComponentModel.DataAnnotations;

namespace webApi.Model
{
    public class CreateNoteDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string Content { get; set; }

        [Required(ErrorMessage = "VideoId không được để trống")]
        public int VideoId { get; set; }

        [Required(ErrorMessage = "UserId không được để trống")]
        public string UserId { get; set; }
    }

    public class UpdateNoteDto
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string Content { get; set; }

        [Required(ErrorMessage = "UserId không được để trống")]
        public string UserId { get; set; }
    }

    public class NoteResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int VideoId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 