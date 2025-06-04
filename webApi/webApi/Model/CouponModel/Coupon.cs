using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webApi.Model.CourseModel;

namespace webApi.Model.CouponModel
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal DiscountAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public int UsageLimit { get; set; }

        public int UsageCount { get; set; } = 0;

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsAutoApply { get; set; } = false;

        public int? CourseId { get; set; }

        public courses Course { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<CouponUsage> CouponUsages { get; set; }
    }

    public class CouponUsage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CouponId { get; set; }

        [Required]
        public string UserId { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CouponId")]
        public Coupon Coupon { get; set; }

        [ForeignKey("UserId")]
        public UserModel.UserInfo User { get; set; }
    }
} 