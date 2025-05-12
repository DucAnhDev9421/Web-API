using System.ComponentModel.DataAnnotations;

namespace webApi.Model.UserModel
{
    public class UserAvatarDto
    {
        [Required(ErrorMessage = "URL ảnh đại diện không được để trống")]
        public string ImageUrl { get; set; }
    }
} 