using Microsoft.EntityFrameworkCore;
namespace webApi.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<courses> courses { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Note> Notes { get; set; }
    }
}
