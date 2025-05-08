using webApi.Model;

namespace webApi.Repositories
{
    public interface ICategoriesRepository
    {
        Task<IEnumerable<Categories>> GetCategoriesAsync();
        Task<Categories> GetCategoriesByIdAsync(int id);
        Task AddCategoriesAsync(Categories Categories);
        Task UpdateCategoriesAsync(Categories Categories);
        Task DeleteCategoriesAsync(int id);
    }
}
