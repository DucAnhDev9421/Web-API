using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.SlideModel;

namespace webApi.Repositories
{
    public class SlideRepository : ISlideRepository
    {
        private readonly ApplicationDbContext _context;

        public SlideRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SlideDto>> GetSlidesAsync(bool? isActive = null)
        {
            var query = _context.Slides.AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(s => s.Order)
                .Select(s => new SlideDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    Subtitle = s.Subtitle,
                    ImageUrl = s.ImageUrl,
                    LinkUrl = s.LinkUrl,
                    Order = s.Order,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<SlideDto?> GetSlideByIdAsync(int id)
        {
            var slide = await _context.Slides.FindAsync(id);
            if (slide == null) return null;

            return new SlideDto
            {
                Id = slide.Id,
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                ImageUrl = slide.ImageUrl,
                LinkUrl = slide.LinkUrl,
                Order = slide.Order,
                IsActive = slide.IsActive,
                CreatedAt = slide.CreatedAt,
                UpdatedAt = slide.UpdatedAt
            };
        }

        public async Task<SlideDto> CreateSlideAsync(CreateSlideDto slideDto)
        {
            var slide = new Slide
            {
                Title = slideDto.Title,
                Subtitle = slideDto.Subtitle,
                ImageUrl = slideDto.ImageUrl,
                LinkUrl = slideDto.LinkUrl,
                Order = slideDto.Order,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Slides.Add(slide);
            await _context.SaveChangesAsync();

            return new SlideDto
            {
                Id = slide.Id,
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                ImageUrl = slide.ImageUrl,
                LinkUrl = slide.LinkUrl,
                Order = slide.Order,
                IsActive = slide.IsActive,
                CreatedAt = slide.CreatedAt,
                UpdatedAt = slide.UpdatedAt
            };
        }

        public async Task<SlideDto?> UpdateSlideAsync(int id, UpdateSlideDto slideDto)
        {
            var slide = await _context.Slides.FindAsync(id);
            if (slide == null) return null;

            slide.Title = slideDto.Title;
            slide.Subtitle = slideDto.Subtitle;
            slide.ImageUrl = slideDto.ImageUrl;
            slide.LinkUrl = slideDto.LinkUrl;
            slide.Order = slideDto.Order;
            slide.IsActive = slideDto.IsActive;
            slide.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SlideDto
            {
                Id = slide.Id,
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                ImageUrl = slide.ImageUrl,
                LinkUrl = slide.LinkUrl,
                Order = slide.Order,
                IsActive = slide.IsActive,
                CreatedAt = slide.CreatedAt,
                UpdatedAt = slide.UpdatedAt
            };
        }

        public async Task<bool> DeleteSlideAsync(int id)
        {
            var slide = await _context.Slides.FindAsync(id);
            if (slide == null) return false;

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSlideStatusAsync(int id, bool isActive)
        {
            var slide = await _context.Slides.FindAsync(id);
            if (slide == null) return false;

            slide.IsActive = isActive;
            slide.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSlideOrderAsync(int id, int newOrder)
        {
            var slide = await _context.Slides.FindAsync(id);
            if (slide == null) return false;

            // Lấy slide hiện tại có order bằng newOrder (nếu có)
            var existingSlide = await _context.Slides
                .FirstOrDefaultAsync(s => s.Order == newOrder && s.Id != id);

            if (existingSlide != null)
            {
                // Hoán đổi order
                existingSlide.Order = slide.Order;
                existingSlide.UpdatedAt = DateTime.UtcNow;
            }

            slide.Order = newOrder;
            slide.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderSlidesAsync(List<int> slideIds)
        {
            if (slideIds == null || !slideIds.Any()) return false;

            // Lấy tất cả slide có ID trong danh sách
            var slides = await _context.Slides
                .Where(s => slideIds.Contains(s.Id))
                .ToListAsync();

            if (slides.Count != slideIds.Count) return false;

            // Cập nhật order cho từng slide theo thứ tự trong danh sách
            for (int i = 0; i < slideIds.Count; i++)
            {
                var slide = slides.First(s => s.Id == slideIds[i]);
                slide.Order = i + 1;
                slide.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
} 