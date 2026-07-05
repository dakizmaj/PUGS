using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetService.Models
{
    public enum ExpenseCategory
    {
        Transport,
        Accommodation,
        Food,
        Tickets,
        Shopping,
        Other
    }

    public class Expense
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Referenca na TravelPlan.Id iz Travel Planning Service-a (druga baza, bez FK)
        [Required]
        public Guid TravelPlanId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public ExpenseCategory Category { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Da li je ovo automatski generisan trosak iz Activity.EstimatedCost,
        // ili rucno unet trosak od strane korisnika (bitno za obracun i za prikaz)
        public bool IsFromActivity { get; set; } = false;

        // Ako je IsFromActivity = true, ovde cuvamo referencu na Activity.Id
        // da bismo mogli da azuriramo/obrisemo ovaj trosak kad se aktivnost promeni/obrise
        public Guid? SourceActivityId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}