using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetService.Dtos
{
    public class CreateExpenseDto
    {
        [Required]
        public Guid TravelPlanId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = "Other";

        [Range(0, double.MaxValue, ErrorMessage = "Iznos ne može biti negativan.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}