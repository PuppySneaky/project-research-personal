using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Film_website.Models;

namespace Film_website.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Movie> Movies { get; set; }
        // Add UserActivity DbSet
        public DbSet<UserActivity> UserActivities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình bảng User
            builder.Entity<User>(entity =>
            {
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayUserName).IsRequired().HasMaxLength(30);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Tạo index unique cho DisplayUserName
                entity.HasIndex(e => e.DisplayUserName).IsUnique();
            });

            // Cấu hình bảng UserActivity
            builder.Entity<UserActivity>(entity =>
            {
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationship
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Index for better query performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.ActivityType);
            });

            // Seed roles cơ bản
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }
    }
}