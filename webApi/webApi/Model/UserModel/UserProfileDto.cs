using System.ComponentModel.DataAnnotations;

namespace webApi.Model.UserModel
{
    public class UserProfileDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string ImageUrl { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public UserStatsDto Stats { get; set; }
    }
    
    public class UserStatsDto
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int FavoriteCourses { get; set; }
        public double AverageProgress { get; set; }
        public DateTime? LastActivityDate { get; set; }
    }
} 