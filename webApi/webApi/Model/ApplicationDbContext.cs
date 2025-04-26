using Microsoft.EntityFrameworkCore;
namespace webApi.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
        {
        }
        public DbSet<courses> courses { get; set; }
    }
}
