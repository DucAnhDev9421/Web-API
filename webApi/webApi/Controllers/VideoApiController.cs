using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using webApi.Repositories;

namespace webApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoApiController : ControllerBase
    {
        private readonly IVideoRepository _videoRepository;

        public VideoApiController(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        [HttpPost("increment-view/{id}")]
        public async Task<IActionResult> IncrementViewCount(int id)
        {
            try
            {
                await _videoRepository.IncrementViewCountAsync(id);
                return Ok(new { message = "View count incremented successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error incrementing view count", error = ex.Message });
            }
        }

        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetVideoProgress(int id, [FromQuery] string userId)
        {
            try
            {
                var progress = await _videoRepository.GetVideoProgressAsync(id, userId);
                if (progress == null)
                {
                    return Ok(new
                    {
                        videoId = id,
                        userId = userId,
                        progressPercentage = 0,
                        lastWatchedAt = DateTime.UtcNow
                    });
                }

                return Ok(new
                {
                    videoId = progress.VideoId,
                    userId = progress.UserId,
                    progressPercentage = progress.ProgressPercentage,
                    lastWatchedAt = progress.LastWatchedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting video progress", error = ex.Message });
            }
        }

        public class TrackVideoProgressRequest
        {
            public string UserId { get; set; }
            public int ProgressPercentage { get; set; }
        }

        [HttpPost("{id}/track")]
        public async Task<IActionResult> TrackVideoProgress(int id, [FromBody] TrackVideoProgressRequest request)
        {
            if (string.IsNullOrEmpty(request.UserId) || request.ProgressPercentage < 0 || request.ProgressPercentage > 100)
            {
                return BadRequest(new { message = "Invalid userId or progressPercentage (must be 0-100)" });
            }
            try
            {
                await _videoRepository.TrackVideoProgressAsync(id, request.UserId, request.ProgressPercentage);
                return Ok(new { message = "Video progress tracked successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error tracking video progress", error = ex.Message });
            }
        }
    }
} 