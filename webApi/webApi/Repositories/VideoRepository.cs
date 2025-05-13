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
        public async Task UpdateVideoOrderAsync(int id, int order)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video != null)
            {
                video.Order = order;
                _context.Entry(video).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateVideoVisibilityAsync(int id, bool isVisible)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video != null)
            {
                video.IsVisible = isVisible;
                _context.Entry(video).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }
        public async Task UpdateVideoMetadataAsync(int id, string title, string description)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video != null)
            {
                video.Title = title;
                video.Description = description;
                _context.Entry(video).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task IncrementViewCountAsync(int id)
        {
            var video = await _context.Set<Video>().FindAsync(id);
            if (video != null)
            {
                video.ViewCount++;
                _context.Entry(video).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<VideoProgress> GetVideoProgressAsync(int videoId, string userId)
        {
            return await _context.Set<VideoProgress>()
                .FirstOrDefaultAsync(vp => vp.VideoId == videoId && vp.UserId == userId);
        }

        public async Task TrackVideoProgressAsync(int videoId, string userId, int progressPercentage)
        {
            var progress = await _context.Set<VideoProgress>()
                .FirstOrDefaultAsync(vp => vp.VideoId == videoId && vp.UserId == userId);
            if (progress == null)
            {
                progress = new VideoProgress
                {
                    VideoId = videoId,
                    UserId = userId,
                    ProgressPercentage = progressPercentage,
                    LastWatchedAt = DateTime.UtcNow
                };
                await _context.Set<VideoProgress>().AddAsync(progress);
            }
            else
            {
                progress.ProgressPercentage = progressPercentage;
                progress.LastWatchedAt = DateTime.UtcNow;
                _context.Entry(progress).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<Video>> GetPopularVideosAsync(int top = 10)
        {
            return await _context.Set<Video>()
                .OrderByDescending(v => v.ViewCount)
                .ThenByDescending(v => v.CreatedAt)
                .Take(top)
                .ToListAsync();
        }

        public async Task<Video> GetNextVideoAsync(int currentVideoId)
        {
            var current = await _context.Set<Video>().FindAsync(currentVideoId);
            if (current == null) return null;
            return await _context.Set<Video>()
                .Where(v => v.CourseId == current.CourseId && v.Order > current.Order)
                .OrderBy(v => v.Order)
                .FirstOrDefaultAsync();
        }

        public async Task<Video> GetPreviousVideoAsync(int currentVideoId)
        {
            var current = await _context.Set<Video>().FindAsync(currentVideoId);
            if (current == null) return null;
            return await _context.Set<Video>()
                .Where(v => v.CourseId == current.CourseId && v.Order < current.Order)
                .OrderByDescending(v => v.Order)
                .FirstOrDefaultAsync();
        }
    }
}
