using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class UpdateActivityDto
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

        [Range(0, double.MaxValue)]
        public decimal EstimatedCost { get; set; }

        [Required]
        public string Status { get; set; } = "Planned";
    }
}