using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Repositories;

namespace webApi.Controllers
{
    [Route("api/notes")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVideoRepository _videoRepository;

        public NotesController(
            INoteRepository noteRepository, 
            IUserRepository userRepository,
            IVideoRepository videoRepository)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _videoRepository = videoRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _noteRepository.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            
            var response = new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                VideoId = note.VideoId,
                UserId = note.UserId,
                UserName = note.User?.Username ?? "Unknown",
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
            
            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var notes = await _noteRepository.GetAllNotesAsync();
            
            var response = notes.Select(note => new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                VideoId = note.VideoId,
                UserId = note.UserId,
                UserName = note.User?.Username ?? "Unknown",
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            }).ToList();
            
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto noteDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra video tồn tại
            var video = await _videoRepository.GetVideoByIdAsync(noteDto.VideoId);
            if (video == null)
            {
                return BadRequest("Video không tồn tại");
            }

            // Kiểm tra user tồn tại
            var user = await _userRepository.GetUserByIdAsync(noteDto.UserId);
            if (user == null)
            {
                return BadRequest("User không tồn tại");
            }

            var note = new Note
            {
                Title = noteDto.Title,
                Content = noteDto.Content,
                VideoId = noteDto.VideoId,
                UserId = noteDto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _noteRepository.AddNoteAsync(note);

            var response = new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                VideoId = note.VideoId,
                UserId = note.UserId,
                UserName = user.Username,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };

            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingNote = await _noteRepository.GetNoteByIdAsync(id);
            if (existingNote == null)
            {
                return NotFound("Không tìm thấy ghi chú");
            }

            // Kiểm tra quyền cập nhật (chỉ người tạo mới được cập nhật)
            if (existingNote.UserId != updateDto.UserId)
            {
                return StatusCode(403, "Bạn không có quyền cập nhật ghi chú này");
            }

            // Cập nhật thông tin ghi chú
            existingNote.Title = updateDto.Title;
            existingNote.Content = updateDto.Content;
            existingNote.UpdatedAt = DateTime.UtcNow;

            await _noteRepository.UpdateNoteAsync(existingNote);
            
            var user = await _userRepository.GetUserByIdAsync(existingNote.UserId);
            
            var response = new NoteResponseDto
            {
                Id = existingNote.Id,
                Title = existingNote.Title,
                Content = existingNote.Content,
                VideoId = existingNote.VideoId,
                UserId = existingNote.UserId,
                UserName = user?.Username ?? "Unknown",
                CreatedAt = existingNote.CreatedAt,
                UpdatedAt = existingNote.UpdatedAt
            };
            
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id, [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var note = await _noteRepository.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound("Ghi chú không tồn tại");
            }

            // Kiểm tra quyền xóa (chỉ người tạo mới được xóa)
            if (note.UserId != userId)
            {
                return Forbid("Bạn không có quyền xóa ghi chú này");
            }

            await _noteRepository.DeleteNoteAsync(id);
            return NoContent();
        }
        
        [HttpGet("video/{videoId}")]
        public async Task<IActionResult> GetNotesByVideoId(int videoId)
        {
            var notes = await _noteRepository.GetNotesByVideoIdAsync(videoId);
            
            var response = notes.Select(note => new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                VideoId = note.VideoId,
                UserId = note.UserId,
                UserName = note.User?.Username ?? "Unknown",
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            }).ToList();
            
            return Ok(response);
        }
        
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotesByUserId(string userId)
        {
            var notes = await _noteRepository.GetNotesByUserIdAsync(userId);
            
            var response = notes.Select(note => new NoteResponseDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                VideoId = note.VideoId,
                UserId = note.UserId,
                UserName = note.User?.Username ?? "Unknown",
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            }).ToList();
            
            return Ok(response);
        }
    }
}
