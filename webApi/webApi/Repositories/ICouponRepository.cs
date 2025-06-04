using webApi.Model.CouponModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace webApi.Repositories
{
    public interface ICouponRepository
    {
        Task<List<Coupon>> GetAllCouponsAsync();
        Task<Coupon> GetCouponByIdAsync(int id);
        Task<Coupon> GetCouponByCodeAsync(string code);
        Task<Coupon> GetActiveAutoApplyCouponAsync();
        Task<Coupon> CreateCouponAsync(Coupon coupon);
        Task<Coupon> UpdateCouponAsync(Coupon coupon);
        Task<bool> DeleteCouponAsync(int id);
        Task<bool> ValidateCouponAsync(string code, string userId);
        Task<bool> UseCouponAsync(string code, string userId);
        Task<List<CouponUsage>> GetCouponUsageHistoryAsync(int couponId);
        Task<bool> ToggleCouponStatusAsync(int id);
        Task<CouponValidationResult> ValidateAndCalculateDiscountAsync(string code, int courseId, string userId);
    }

    public class CouponDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoApply { get; set; }
        public int? CourseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCouponDto
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public bool IsAutoApply { get; set; }
        public int? CourseId { get; set; }
    }

    public class UpdateCouponDto
    {
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsageLimit { get; set; }
        public bool IsActive { get; set; }
        public bool IsAutoApply { get; set; }
        public int? CourseId { get; set; }
    }

    public class CouponUsageDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime UsedAt { get; set; }
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public CouponDto Coupon { get; set; }
    }
} 