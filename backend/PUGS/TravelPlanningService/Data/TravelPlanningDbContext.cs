using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TravelPlanningService.Models;

namespace TravelPlanningService.Data
{
    public class TravelPlanningDbContext : DbContext
    {
        public TravelPlanningDbContext(DbContextOptions<TravelPlanningDbContext> options) : base(options) { }

        public DbSet<TravelPlan> TravelPlans { get; set; } = null!;
        public DbSet<Destination> Destinations { get; set; } = null!;
        public DbSet<Activity> Activities { get; set; } = null!;
        public DbSet<ChecklistItem> ChecklistItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cascade delete: brisanje TravelPlan-a briše sve povezane entitete
            modelBuilder.Entity<Destination>()
                .HasOne(d => d.TravelPlan)
                .WithMany(p => p.Destinations)
                .HasForeignKey(d => d.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Activity>()
                .HasOne(a => a.TravelPlan)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChecklistItem>()
                .HasOne(c => c.TravelPlan)
                .WithMany(p => p.ChecklistItems)
                .HasForeignKey(c => c.TravelPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Budžet ne sme biti negativan - dodatni check constraint na nivou baze
            modelBuilder.Entity<TravelPlan>()
                .ToTable(t => t.HasCheckConstraint("CK_TravelPlan_Budget_NonNegative", "[Budget] >= 0"));

            modelBuilder.Entity<Activity>()
                .ToTable(t => t.HasCheckConstraint("CK_Activity_EstimatedCost_NonNegative", "[EstimatedCost] >= 0"));

            modelBuilder.Entity<TravelPlan>()
                .ToTable(t => t.HasCheckConstraint("CK_TravelPlan_EndDate_After_StartDate", "[EndDate] >= [StartDate]"));

            modelBuilder.Entity<Destination>()
                .ToTable(t => t.HasCheckConstraint("CK_Destination_DepartureAfterArrival", "[DepartureDate] >= [ArrivalDate]"));
        }
    }
}