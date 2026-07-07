using Microsoft.EntityFrameworkCore;
using SharingService.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SharingService.Data
{
    public class SharingDbContext : DbContext
    {
        public SharingDbContext(DbContextOptions<SharingDbContext> options) : base(options) { }

        public DbSet<ShareLink> ShareLinks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShareLink>()
                .HasIndex(s => s.Token)
                .IsUnique();

            modelBuilder.Entity<ShareLink>()
                .HasIndex(s => s.TravelPlanId);
        }
    }
}