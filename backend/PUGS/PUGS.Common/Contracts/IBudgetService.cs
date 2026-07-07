using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace PUGS.Common.Contracts
{
    public class BudgetSummaryResult
    {
        public decimal TotalSpent { get; set; }
        public List<CategoryTotalResult> ByCategory { get; set; } = new();
    }

    public class CategoryTotalResult
    {
        public string Category { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public interface IBudgetService : IService
    {
        Task RecordActivityExpenseAsync(Guid travelPlanId, Guid activityId, string activityName, decimal estimatedCost, DateTime activityDate);
        Task UpdateActivityExpenseAsync(Guid activityId, string activityName, decimal newEstimatedCost, DateTime activityDate);
        Task RemoveActivityExpenseAsync(Guid activityId);
        Task<BudgetSummaryResult> GetBudgetSummaryAsync(Guid travelPlanId);
        Task DeleteAllExpensesForPlanAsync(Guid travelPlanId);
    }
}