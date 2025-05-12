using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.UserModel;

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
            return await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Video)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            return await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Video)
                .ToListAsync();
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
            return await _context.Notes
                .Include(n => n.User)
                .Include(n => n.Video)
                .Where(n => n.VideoId == videoId)
                .ToListAsync();
        }

        public async Task<List<Note>> GetNotesByUserIdAsync(string userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
