using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Note> GetNoteByIdAsync(int id)
        {
            return await _context.Notes.FindAsync(id);
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _context.Notes.ToListAsync();
        }

        public async Task AddNoteAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            _context.Entry(note).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Note>> GetNotesByVideoIdAsync(int videoId)
        {
            return await _context.Notes.Where(n => n.VideoId == videoId).ToListAsync();
        }
    }
}
