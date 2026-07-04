using System;
using System.ComponentModel.DataAnnotations;

namespace TravelPlanningService.Dtos
{
    public class CreateDestinationDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public DateTime ArrivalDate { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}