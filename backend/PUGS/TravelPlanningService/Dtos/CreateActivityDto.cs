using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class CreateActivityDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        public TimeSpan? Time { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Procenjeni trošak ne može biti negativan.")]
        public decimal EstimatedCost { get; set; }

        // Opciono pri kreiranju - default je "Planned" ako se ne posalje
        public string? Status { get; set; }
    }
}