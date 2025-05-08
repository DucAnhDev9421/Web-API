using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class coursesRepository : IcoursesRepository
    {
        private readonly ApplicationDbContext _context;

        public coursesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<courses>> GetcoursesAsync()
        {
            return await _context.courses.ToListAsync();
        }

        public async Task<courses> GetcoursesByIdAsync(int id)
        {
            return await _context.courses.FindAsync(id);
        }

        public async Task AddcoursesAsync(courses courses)
        {
            _context.courses.Add(courses);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatecoursesAsync(courses courses)
        {
            _context.Entry(courses).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeletecoursesAsync(int id)
        {
            var courses = await _context.courses.FindAsync(id);
            if (courses != null)
            {
                _context.courses.Remove(courses);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<courses>> GetFreecoursesAsync()
        {
            return await _context.courses
                .Where(c => c.Price == 0)  //  Lọc các khóa học có giá bằng 0
                .ToListAsync();
        }
    }
}
