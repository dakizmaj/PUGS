using System;
using BudgetService.Dtos;
using BudgetService.Models;

namespace BudgetService.Mapping
{
    public static class ExpenseMapper
    {
        public static ExpenseResponseDto ToResponseDto(Expense expense)
        {
            return new ExpenseResponseDto
            {
                Id = expense.Id,
                TravelPlanId = expense.TravelPlanId,
                Name = expense.Name,
                Category = expense.Category.ToString(),
                Amount = expense.Amount,
                Date = expense.Date,
                Description = expense.Description,
                IsFromActivity = expense.IsFromActivity
            };
        }

        public static bool TryFromCreateDto(CreateExpenseDto dto, out Expense? expense)
        {
            expense = null;

            if (!Enum.TryParse<ExpenseCategory>(dto.Category, true, out var category))
                return false;

            expense = new Expense
            {
                TravelPlanId = dto.TravelPlanId,
                Name = dto.Name,
                Category = category,
                Amount = dto.Amount,
                Date = dto.Date,
                Description = dto.Description,
                IsFromActivity = false
            };

            return true;
        }

        public static bool ApplyUpdate(Expense expense, UpdateExpenseDto dto)
        {
            if (!Enum.TryParse<ExpenseCategory>(dto.Category, true, out var category))
                return false;

            expense.Name = dto.Name;
            expense.Category = category;
            expense.Amount = dto.Amount;
            expense.Date = dto.Date;
            expense.Description = dto.Description;

            return true;
        }
    }
}