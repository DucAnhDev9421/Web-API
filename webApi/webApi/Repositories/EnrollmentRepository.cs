using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using webApi.Model;
using System.Collections.Generic;
using System.Linq;

namespace webApi.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly ApplicationDbContext _context;
        public EnrollmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EnrollAsync(string userId, int courseId)
        {
            var exists = await _context.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (exists) return false;

            var enrollment = new Enrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = System.DateTime.UtcNow
            };
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Enrollment>> GetEnrollmentsByUserAsync(string userId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UnenrollAsync(string userId, int courseId)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
            if (enrollment == null) return false;
            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 