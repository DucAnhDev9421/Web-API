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
        private readonly INoteRepository _noteRepository;

        public VideosController(IVideoRepository videoRepository, INoteRepository noteRepository)
        {
            _videoRepository = videoRepository;
            _noteRepository = noteRepository;
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
        [HttpGet("{id}/notes")]
        public async Task<IActionResult> GetVideoNotes(int id)
        {
            try
            {
                var video = await _videoRepository.GetVideoByIdAsync(id);
                if (video == null)
                {
                    return NotFound("Video not found");
                }

                var notes = await _noteRepository.GetNotesByVideoIdAsync(id);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPatch("{id}/visibility")]
        public async Task<IActionResult> UpdateVideoVisibility(int id, [FromBody] bool isVisible)
        {
            try
            {
                var existingVideo = await _videoRepository.GetVideoByIdAsync(id);
                if (existingVideo == null)
                {
                    return NotFound();
                }

                await _videoRepository.UpdateVideoVisibilityAsync(id, isVisible);
                return NoContent();
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
    }
}
