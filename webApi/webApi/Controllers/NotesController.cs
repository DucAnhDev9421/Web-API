using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using webApi.Model;
using webApi.Repositories;

namespace webApi.Controllers
{
    [Route("api/notes")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteRepository _noteRepository;

        public NotesController(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _noteRepository.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return Ok(note);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var notes = await _noteRepository.GetAllNotesAsync();
            return Ok(notes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _noteRepository.AddNoteAsync(note);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != note.Id)
            {
                return BadRequest("Note ID in the URL does not match the ID in the request body.");
            }

            var existingNote = await _noteRepository.GetNoteByIdAsync(id);
            if (existingNote == null)
            {
                return NotFound();
            }

            existingNote.Title = note.Title;
            existingNote.Content = note.Content;

            await _noteRepository.UpdateNoteAsync(existingNote);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _noteRepository.GetNoteByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            await _noteRepository.DeleteNoteAsync(id);
            return NoContent();
        }
    }
}
