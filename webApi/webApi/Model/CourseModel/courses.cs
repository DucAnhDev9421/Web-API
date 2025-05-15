using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using webApi.Model.CategoryModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webApi.Model.CourseModel
{
    public enum CourseStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Hidden = 3
    }

    public enum CourseLevel
    {
        Beginner = 1,    // Cơ bản
        Intermediate = 2, // Trung cấp
        Advanced = 3     // Nâng cao
    }

    public class courses
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public CourseStatus Status { get; set; } = CourseStatus.Pending;

        [NotMapped]
        public string StatusText => Status.ToString();

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [NotMapped]
        public string LevelText => Level.ToString();

        // Thêm CategoryId và navigation property
        public int? CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        [JsonIgnore]
        public Categories? Category { get; set; }
    }


}
