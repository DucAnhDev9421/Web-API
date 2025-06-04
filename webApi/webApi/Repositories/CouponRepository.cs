using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.CouponModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webApi.Repositories
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coupon>> GetAllCouponsAsync()
        {
            return await _context.Coupons
                .Include(c => c.CouponUsages)
                .ToListAsync();
        }

        public async Task<Coupon> GetCouponByIdAsync(int id)
        {
            return await _context.Coupons
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Coupon> GetCouponByCodeAsync(string code)
        {
            return await _context.Coupons
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        public async Task<Coupon> GetActiveAutoApplyCouponAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Coupons
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(c => 
                    c.IsActive && 
                    c.IsAutoApply && 
                    c.StartDate <= now && 
                    c.EndDate >= now);
        }

        public async Task<Coupon> CreateCouponAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<Coupon> UpdateCouponAsync(Coupon coupon)
        {
            _context.Entry(coupon).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> DeleteCouponAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
                return false;

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ValidateCouponAsync(string code, string userId)
        {
            var now = DateTime.UtcNow;
            var coupon = await _context.Coupons
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(c => 
                    c.Code == code && 
                    c.IsActive && 
                    c.StartDate <= now && 
                    c.EndDate >= now);

            if (coupon == null)
                return false;

            // Kiểm tra số lần sử dụng
            if (coupon.UsageLimit > 0 && coupon.UsageCount >= coupon.UsageLimit)
                return false;

            // Kiểm tra xem user đã sử dụng coupon này chưa
            var hasUsed = await _context.CouponUsages
                .AnyAsync(cu => cu.CouponId == coupon.Id && cu.UserId == userId);

            return !hasUsed;
        }

        public async Task<bool> UseCouponAsync(string code, string userId)
        {
            var coupon = await GetCouponByCodeAsync(code);
            if (coupon == null)
                return false;

            var isValid = await ValidateCouponAsync(code, userId);
            if (!isValid)
                return false;

            // Tăng số lần sử dụng
            coupon.UsageCount++;
            
            // Thêm vào lịch sử sử dụng
            var usage = new CouponUsage
            {
                CouponId = coupon.Id,
                UserId = userId,
                UsedAt = DateTime.UtcNow
            };
            _context.CouponUsages.Add(usage);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CouponUsage>> GetCouponUsageHistoryAsync(int couponId)
        {
            return await _context.CouponUsages
                .Include(cu => cu.User)
                .Where(cu => cu.CouponId == couponId)
                .OrderByDescending(cu => cu.UsedAt)
                .ToListAsync();
        }

        public async Task<bool> ToggleCouponStatusAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null)
                return false;

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CouponValidationResult> ValidateAndCalculateDiscountAsync(string code, int courseId, string userId)
        {
            var result = new CouponValidationResult
            {
                IsValid = false,
                Message = string.Empty
            };

            // Lấy thông tin khóa học
            var course = await _context.courses.FindAsync(courseId);
            if (course == null)
            {
                result.Message = "Không tìm thấy khóa học";
                return result;
            }

            result.OriginalPrice = course.Price;

            // Lấy thông tin coupon
            var coupon = await _context.Coupons
                .Include(c => c.CouponUsages)
                .FirstOrDefaultAsync(c => c.Code == code);

            if (coupon == null)
            {
                result.Message = "Mã coupon không tồn tại";
                return result;
            }

            // Kiểm tra thời hạn
            var now = DateTime.UtcNow;
            if (now < coupon.StartDate || now > coupon.EndDate)
            {
                result.Message = "Coupon đã hết hạn hoặc chưa đến thời gian áp dụng";
                return result;
            }

            // Kiểm tra trạng thái
            if (!coupon.IsActive)
            {
                result.Message = "Coupon đã bị vô hiệu hóa";
                return result;
            }

            // Kiểm tra số lần sử dụng
            if (coupon.UsageLimit > 0 && coupon.UsageCount >= coupon.UsageLimit)
            {
                result.Message = "Coupon đã hết lượt sử dụng";
                return result;
            }

            // Kiểm tra xem coupon có áp dụng cho khóa học này không
            if (coupon.CourseId.HasValue && coupon.CourseId.Value != courseId)
            {
                result.Message = "Coupon không áp dụng cho khóa học này";
                return result;
            }

            // Kiểm tra xem user đã sử dụng coupon này chưa
            var hasUsed = await _context.CouponUsages
                .AnyAsync(cu => cu.CouponId == coupon.Id && cu.UserId == userId);
            if (hasUsed)
            {
                result.Message = "Bạn đã sử dụng coupon này";
                return result;
            }

            // Tính toán giảm giá
            result.DiscountAmount = coupon.DiscountAmount;
            result.FinalPrice = Math.Max(0, course.Price - coupon.DiscountAmount);
            result.IsValid = true;
            result.Message = "Coupon hợp lệ";
            result.Coupon = new CouponDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Description = coupon.Description,
                DiscountAmount = coupon.DiscountAmount,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageLimit = coupon.UsageLimit,
                UsageCount = coupon.UsageCount,
                IsActive = coupon.IsActive,
                IsAutoApply = coupon.IsAutoApply,
                CourseId = coupon.CourseId,
                CreatedAt = coupon.CreatedAt,
                UpdatedAt = coupon.UpdatedAt
            };

            return result;
        }
    }
} 