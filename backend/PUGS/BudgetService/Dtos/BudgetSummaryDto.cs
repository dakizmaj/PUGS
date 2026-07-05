using System;
using System.Collections.Generic;

namespace BudgetService.Dtos
{
    public class BudgetSummaryDto
    {
        public Guid TravelPlanId { get; set; }
        public decimal PlannedBudget { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<CategoryTotalDto> ByCategory { get; set; } = new();
    }

    public class CategoryTotalDto
    {
        public string Category { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}