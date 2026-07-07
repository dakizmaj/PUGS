using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BudgetService.Data;
using BudgetService.Models;
using PUGS.Common.Contracts;

namespace BudgetService.Services
{
    public class BudgetRemotingService : IBudgetService
    {
        private readonly string _connectionString;

        public BudgetRemotingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private BudgetDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new BudgetDbContext(optionsBuilder.Options);
        }

        public async Task RecordActivityExpenseAsync(Guid travelPlanId, Guid activityId, string activityName, decimal estimatedCost, DateTime activityDate)
        {
            using var context = CreateContext();

            var existing = await context.Expenses
                .FirstOrDefaultAsync(e => e.SourceActivityId == activityId);

            if (existing != null)
                return;

            var expense = new Expense
            {
                TravelPlanId = travelPlanId,
                Name = $"Aktivnost: {activityName}",
                Category = ExpenseCategory.Other,
                Amount = estimatedCost,
                Date = activityDate,
                Description = "Automatski generisano iz aktivnosti",
                IsFromActivity = true,
                SourceActivityId = activityId
            };

            context.Expenses.Add(expense);
            await context.SaveChangesAsync();
        }

        public async Task UpdateActivityExpenseAsync(Guid activityId, string activityName, decimal newEstimatedCost, DateTime activityDate)
        {
            using var context = CreateContext();

            var expense = await context.Expenses
                .FirstOrDefaultAsync(e => e.SourceActivityId == activityId);

            if (expense == null)
                return;

            expense.Name = $"Aktivnost: {activityName}";
            expense.Amount = newEstimatedCost;
            expense.Date = activityDate;

            await context.SaveChangesAsync();
        }

        public async Task RemoveActivityExpenseAsync(Guid activityId)
        {
            using var context = CreateContext();

            var expense = await context.Expenses
                .FirstOrDefaultAsync(e => e.SourceActivityId == activityId);

            if (expense == null)
                return;

            context.Expenses.Remove(expense);
            await context.SaveChangesAsync();
        }

        public async Task<BudgetSummaryResult> GetBudgetSummaryAsync(Guid travelPlanId)
        {
            using var context = CreateContext();

            var expenses = await context.Expenses
                .Where(e => e.TravelPlanId == travelPlanId)
                .ToListAsync();

            return new BudgetSummaryResult
            {
                TotalSpent = expenses.Sum(e => e.Amount),
                ByCategory = expenses
                    .GroupBy(e => e.Category)
                    .Select(g => new CategoryTotalResult { Category = g.Key.ToString(), Total = g.Sum(e => e.Amount) })
                    .ToList()
            };
        }

        public async Task DeleteAllExpensesForPlanAsync(Guid travelPlanId)
        {
            using var context = CreateContext();

            var expenses = await context.Expenses
                .Where(e => e.TravelPlanId == travelPlanId)
                .ToListAsync();

            context.Expenses.RemoveRange(expenses);
            await context.SaveChangesAsync();
        }
    }
}