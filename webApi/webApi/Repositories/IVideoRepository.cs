using webApi.Model;

namespace webApi.Repositories
{
    public interface IVideoRepository
    {
        Task<Video> GetVideoByIdAsync(int id);
        Task AddVideoAsync(Video video);
        Task UpdateVideoAsync(Video video); // Thêm phương thức này
        Task DeleteVideoAsync(int id);
    }
}
