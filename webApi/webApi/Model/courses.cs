using System.ComponentModel.DataAnnotations;

namespace webApi.Model
{
    public class courses
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
