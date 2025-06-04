using webApi.Model.SlideModel;

namespace webApi.Repositories
{
    public class SlideDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSlideDto
    {
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public int Order { get; set; }
    }

    public class UpdateSlideDto
    {
        public string Title { get; set; }
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public string LinkUrl { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    public interface ISlideRepository
    {
        // Lấy danh sách slide (có thể lọc theo trạng thái active)
        Task<List<SlideDto>> GetSlidesAsync(bool? isActive = null);

        // Lấy chi tiết một slide
        Task<SlideDto?> GetSlideByIdAsync(int id);

        // Tạo slide mới
        Task<SlideDto> CreateSlideAsync(CreateSlideDto slide);

        // Cập nhật slide
        Task<SlideDto?> UpdateSlideAsync(int id, UpdateSlideDto slide);

        // Xóa slide
        Task<bool> DeleteSlideAsync(int id);

        // Cập nhật trạng thái active của slide
        Task<bool> UpdateSlideStatusAsync(int id, bool isActive);

        // Cập nhật thứ tự của slide
        Task<bool> UpdateSlideOrderAsync(int id, int newOrder);

        // Sắp xếp lại thứ tự của tất cả slide
        Task<bool> ReorderSlidesAsync(List<int> slideIds);
    }
} 