using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class CreateTravelPlanDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Budžet ne može biti negativan.")]
        public decimal Budget { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}