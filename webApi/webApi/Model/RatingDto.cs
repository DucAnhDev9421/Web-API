using System.ComponentModel.DataAnnotations;

namespace webApi.Model
{
    public class CreateRatingDto
    {
        [Required(ErrorMessage = "UserId không được để trống")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "CourseId không được để trống")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "RatingValue không được để trống")]
        [Range(1, 5, ErrorMessage = "RatingValue phải từ 1 đến 5")]
        public int RatingValue { get; set; }

        public string Comment { get; set; }
    }

    public class UpdateRatingDto
    {
        [Required(ErrorMessage = "RatingValue không được để trống")]
        [Range(1, 5, ErrorMessage = "RatingValue phải từ 1 đến 5")]
        public int RatingValue { get; set; }

        public string Comment { get; set; }
    }

    public class RatingResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string ImageUrl { get; set; }
        public int CourseId { get; set; }
        public int RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 