using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webApi.Model;
using webApi.Repositories;

namespace webApi.Controllers
{
    [Route("api/videos")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly IVideoRepository _videoRepository;

        public VideosController(IVideoRepository videoRepository)
        {
            _videoRepository = videoRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVideoById(int id)
        {
            try
            {
                var video = await _videoRepository.GetVideoByIdAsync(id);

                if (video == null)
                {
                    return NotFound();  // Trả về 404 nếu không tìm thấy
                }

                return Ok(video);  // Trả về 200 và thông tin video
            }
            catch (Exception ex)
            {
                // Log lỗi (quan trọng cho debug)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateVideo([FromBody] Video video)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _videoRepository.AddVideoAsync(video);
                return CreatedAtAction(nameof(GetVideoById), new { id = video.Id }, video);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideo(int id, [FromBody] Video video)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != video.Id)
                {
                    return BadRequest("Video ID in the URL does not match the ID in the request body.");
                }

                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                // Cập nhật các thuộc tính của existingVideo từ đối tượng 'video'
                existingVideo.Title = video.Title;
                existingVideo.Description = video.Description;
                existingVideo.CourseId = video.CourseId;
                // ... cập nhật các thuộc tính khác ...

                await _videoRepository.UpdateVideoAsync(existingVideo); // Truyền entity đã được cập nhật
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/order")]
        public async Task<IActionResult> UpdateVideoOrder(int id, [FromBody] int order)
        {
            try
            {
                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                await _videoRepository.UpdateVideoOrderAsync(id, order);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            try
            {
                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                await _videoRepository.DeleteVideoAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/visibility")]
        public async Task<IActionResult> UpdateVideoVisibility(int id, [FromBody] VideoVisibilityDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Request body is required");
                }
                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                await _videoRepository.UpdateVideoVisibilityAsync(id, dto.IsVisible);
                // Lấy lại video đã cập nhật
                var updatedVideo = await _videoRepository.GetVideoByIdAsync(id);
                return Ok(updatedVideo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("{id}/metadata")]  // New PATCH endpoint
        public async Task<IActionResult> UpdateVideoMetadata(int id, [FromBody] VideoMetadataDto metadata)
        {
            try
            {
                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                await _videoRepository.UpdateVideoMetadataAsync(id, metadata.Title, metadata.Description);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("upload-url")]
        public IActionResult GetUploadUrl()
        {
            // Giả lập URL upload tạm thời
            var tempUploadUrl = "https://youtube.com/upload?token=some-temp-token";
            return Ok(new { uploadUrl = tempUploadUrl, expiresIn = 600 }); // expiresIn: giây
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularVideos([FromQuery] int top = 10)
        {
            var videos = await _videoRepository.GetPopularVideosAsync(top);
            var result = videos.Select(v => new {
                v.Id,
                v.Title,
                v.ViewCount,
                v.Thumbnail,
                v.Duration,
                v.CreatedAt
            });
            return Ok(result);
        }

        public class VideoValidateRequest
        {
            public string FileName { get; set; }
            public int Duration { get; set; } // giây
        }

        [HttpPost("validate")]
        public IActionResult ValidateVideo([FromBody] VideoValidateRequest request)
        {
            var allowedExtensions = new[] { ".mp4", ".mov", ".avi", ".mkv" };
            var maxDuration = 60 * 60 * 2; // 2 giờ
            var ext = System.IO.Path.GetExtension(request.FileName)?.ToLower();
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return Ok(new { isValid = false, message = "Định dạng video không hỗ trợ" });
            }
            if (request.Duration <= 0 || request.Duration > maxDuration)
            {
                return Ok(new { isValid = false, message = "Thời lượng video phải lớn hơn 0 và không vượt quá 2 giờ" });
            }
            return Ok(new { isValid = true, message = "Video hợp lệ" });
        }

        [HttpGet("{id}/next")]
        public async Task<IActionResult> GetNextVideo(int id)
        {
            var nextVideo = await _videoRepository.GetNextVideoAsync(id);
            if (nextVideo == null)
                return NotFound(new { message = "Không có bài học tiếp theo" });
            return Ok(nextVideo);
        }

        [HttpGet("{id}/prev")]
        public async Task<IActionResult> GetPreviousVideo(int id)
        {
            var prevVideo = await _videoRepository.GetPreviousVideoAsync(id);
            if (prevVideo == null)
                return NotFound(new { message = "Không có bài học trước đó" });
            return Ok(prevVideo);
        }
    }
}
