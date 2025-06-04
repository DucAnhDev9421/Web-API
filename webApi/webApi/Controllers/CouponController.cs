using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using webApi.Repositories;
using webApi.Model.CouponModel;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;

namespace webApi.Controllers
{
    [Route("api/coupons")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllCoupons()
        {
            try
            {
                var coupons = await _couponRepository.GetAllCouponsAsync();
                return Ok(coupons);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetCouponById(int id)
        {
            try
            {
                var coupon = await _couponRepository.GetCouponByIdAsync(id);
                if (coupon == null)
                    return NotFound("Coupon not found");

                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]

        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto createCouponDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (createCouponDto.IsAutoApply)
                {
                    var existingAutoApplyCoupon = await _couponRepository.GetActiveAutoApplyCouponAsync();
                    if (existingAutoApplyCoupon != null)
                    {
                        return BadRequest("Đã có một coupon tự động áp dụng đang hoạt động. Vui lòng tắt coupon đó trước khi tạo mới.");
                    }
                }

                var coupon = new Coupon
                {
                    Code = createCouponDto.Code,
                    Description = createCouponDto.Description,
                    DiscountAmount = createCouponDto.DiscountAmount,
                    StartDate = createCouponDto.StartDate,
                    EndDate = createCouponDto.EndDate,
                    UsageLimit = createCouponDto.UsageLimit,
                    IsActive = true,
                    IsAutoApply = createCouponDto.IsAutoApply,
                    CourseId = createCouponDto.CourseId
                };

                var createdCoupon = await _couponRepository.CreateCouponAsync(coupon);
                return CreatedAtAction(nameof(GetCouponById), new { id = createdCoupon.Id }, createdCoupon);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] UpdateCouponDto updateCouponDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCoupon = await _couponRepository.GetCouponByIdAsync(id);
                if (existingCoupon == null)
                    return NotFound("Coupon not found");

                if (updateCouponDto.IsAutoApply && !existingCoupon.IsAutoApply)
                {
                    var existingAutoApplyCoupon = await _couponRepository.GetActiveAutoApplyCouponAsync();
                    if (existingAutoApplyCoupon != null && existingAutoApplyCoupon.Id != id)
                    {
                        return BadRequest("Đã có một coupon tự động áp dụng đang hoạt động. Vui lòng tắt coupon đó trước khi cập nhật.");
                    }
                }

                existingCoupon.Description = updateCouponDto.Description;
                existingCoupon.DiscountAmount = updateCouponDto.DiscountAmount;
                existingCoupon.StartDate = updateCouponDto.StartDate;
                existingCoupon.EndDate = updateCouponDto.EndDate;
                existingCoupon.UsageLimit = updateCouponDto.UsageLimit;
                existingCoupon.IsActive = updateCouponDto.IsActive;
                existingCoupon.IsAutoApply = updateCouponDto.IsAutoApply;
                existingCoupon.CourseId = updateCouponDto.CourseId;
                existingCoupon.UpdatedAt = DateTime.UtcNow;

                var updatedCoupon = await _couponRepository.UpdateCouponAsync(existingCoupon);
                return Ok(updatedCoupon);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteCoupon(int id)
        {
            try
            {
                var result = await _couponRepository.DeleteCouponAsync(id);
                if (!result)
                    return NotFound("Coupon not found");

                return Ok(new { message = "Coupon deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/toggle")]

        public async Task<IActionResult> ToggleCouponStatus(int id)
        {
            try
            {
                var result = await _couponRepository.ToggleCouponStatusAsync(id);
                if (!result)
                    return NotFound("Coupon not found");

                return Ok(new { message = "Coupon status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}/usage")]

        public async Task<IActionResult> GetCouponUsageHistory(int id)
        {
            try
            {
                var usageHistory = await _couponRepository.GetCouponUsageHistoryAsync(id);
                return Ok(usageHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var result = await _couponRepository.ValidateAndCalculateDiscountAsync(
                    request.Code,
                    request.CourseId,
                    userId
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("use")]
        [Authorize]
        public async Task<IActionResult> UseCoupon([FromBody] UseCouponRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authenticated");

                var validationResult = await _couponRepository.ValidateAndCalculateDiscountAsync(
                    request.Code,
                    request.CourseId,
                    userId
                );

                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Message);
                }

                var result = await _couponRepository.UseCouponAsync(request.Code, userId);
                if (!result)
                    return BadRequest("Invalid or expired coupon");

                return Ok(new { 
                    message = "Coupon used successfully",
                    discountAmount = validationResult.DiscountAmount,
                    finalPrice = validationResult.FinalPrice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("auto-apply")]
        public async Task<IActionResult> GetActiveAutoApplyCoupon()
        {
            try
            {
                var coupon = await _couponRepository.GetActiveAutoApplyCouponAsync();
                if (coupon == null)
                    return NotFound("Không có coupon tự động áp dụng nào đang hoạt động");

                return Ok(coupon);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    public class ValidateCouponRequest
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int CourseId { get; set; }
    }

    public class UseCouponRequest
    {
        [Required]
        public string Code { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}