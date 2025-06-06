using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using webApi.Model.CourseModel;

namespace webApi.Model.UserModel
{
    public class UserInfo
    {
        [Key]
        [Required]
        [StringLength(450)] // Độ dài tối đa cho nvarchar trong SQL Server
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("email_addresses")]
        [NotMapped]
        public List<EmailAddress> EmailAddresses { get; set; }

        [Required]
        [StringLength(256)]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [StringLength(100)]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [StringLength(100)]
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [StringLength(100)]
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [StringLength(500)]
        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        [JsonPropertyName("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [StringLength(50)]
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("public_metadata")]
        [NotMapped]
        public PublicMetadata PublicMetadata { get; set; }

        [JsonPropertyName("private_metadata")]
        [NotMapped]
        public PrivateMetadata PrivateMetadata { get; set; }

        [StringLength(100)]
        public string JobTitle { get; set; }

        [StringLength(1000)]
        public string Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; }
        public virtual ICollection<courses> Courses { get; set; }
    }

    public class EmailAddress
    {
        [JsonPropertyName("email_address")]
        public string Email { get; set; }
    }

    public class PublicMetadata
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class PrivateMetadata
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
} 