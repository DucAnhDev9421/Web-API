using Microsoft.EntityFrameworkCore;
using webApi.Model;

namespace webApi.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Video> GetVideoByIdAsync(int id)
        {
            return await _context.Set<Video>().FindAsync(id);
        }
        public async Task AddVideoAsync(Video video)
        {
            _context.Set<Video>().Add(video);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVideoAsync(Video video)
        {
            _context.Entry(video).State = EntityState.Modified; // Đánh dấu entity là đã sửa đổi
            await _context.SaveChangesAsync();
        }

        public async Task DeleteVideoAsync(int id)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video != null)
            {
                _context.Set<Video>().Remove(video);
                await _context.SaveChangesAsync();
            }
        }
    }
}
