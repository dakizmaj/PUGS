using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetService.Dtos
{
    public class UpdateExpenseDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = "Other";

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}