using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}