using BudgetService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BudgetService.Data
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) { }

        public DbSet<Expense> Expenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .ToTable(t => t.HasCheckConstraint("CK_Expense_Amount_NonNegative", "[Amount] >= 0"));

            // Indeks za brzo pretrazivanje troskova po planu
            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.TravelPlanId);
        }
    }
}