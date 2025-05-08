using Microsoft.EntityFrameworkCore;
namespace webApi.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options) : base(options)
        {
        }
        public DbSet<courses> courses { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Video> Videos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<courses>()
                .Property(c => c.Price)
                .HasPrecision(18, 2); // Hoặc HasColumnType("decimal(18,2)");
        }
    }
}
