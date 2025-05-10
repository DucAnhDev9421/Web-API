using System.ComponentModel.DataAnnotations;

namespace webApi.Model.UserModel
{
    public class UserUpdateDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string ImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }
    }
} 