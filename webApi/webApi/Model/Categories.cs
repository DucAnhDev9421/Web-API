using System.ComponentModel.DataAnnotations;

namespace webApi.Model
{
    public class Categories
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
