using Microsoft.EntityFrameworkCore;
using webApi.Model;
using webApi.Model.CategoryModel;
namespace webApi.Repositories
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly ApplicationDbContext _context;
        public CategoriesRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Categories>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
        public async Task<Categories> GetCategoriesByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddCategoriesAsync(Categories Categories)
        {
            _context.Categories.Add(Categories);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoriesAsync(Categories Categories)
        {
            _context.Entry(Categories).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoriesAsync(int id)
        {
            var Categories = await _context.Categories.FindAsync(id);
            if (Categories != null)
            {
                _context.Categories.Remove(Categories);
                await _context.SaveChangesAsync();
            }
        }
    }
}

