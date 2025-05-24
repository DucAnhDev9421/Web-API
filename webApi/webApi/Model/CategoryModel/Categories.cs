using System.ComponentModel.DataAnnotations;

namespace webApi.Model.CategoryModel
{
    public class Categories
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
