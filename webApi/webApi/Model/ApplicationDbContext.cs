using Microsoft.EntityFrameworkCore;
using webApi.Model.CategoryModel;
using webApi.Model.CourseModel;
using webApi.Model.UserModel;
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
        public DbSet<UserInfo> Users { get; set; }
        public DbSet<UserCourseProgress> UserCourseProgress { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<UserFavoriteCourse> UserFavoriteCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserInfo>()
                .Property(u => u.Id)
                .IsRequired();

            modelBuilder.Entity<UserInfo>()
                .Property(u => u.Email)
                .IsRequired();

            modelBuilder.Entity<UserCourseProgress>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCourseProgress>()
                .HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<UserFavoriteCourse>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavoriteCourse>()
                .HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Create a unique index for UserId and CourseId combination to prevent duplicates
            modelBuilder.Entity<UserFavoriteCourse>()
                .HasIndex(f => new { f.UserId, f.CourseId })
                .IsUnique();
        }
    }
}
