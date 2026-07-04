using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanningService.Models
{
    public class Destination
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelPlanId { get; set; }

        [ForeignKey(nameof(TravelPlanId))]
        public TravelPlan? TravelPlan { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        // String je dovoljan za lokaciju (Q&A #24)
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