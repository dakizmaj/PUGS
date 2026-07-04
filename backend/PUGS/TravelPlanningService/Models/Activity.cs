using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanningService.Models
{
    public enum ActivityStatus
    {
        Planned,
        Reserved,
        Completed,
        Cancelled
    }

    public class Activity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelPlanId { get; set; }

        [ForeignKey(nameof(TravelPlanId))]
        public TravelPlan? TravelPlan { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        // Vreme aktivnosti u okviru dana
        public TimeSpan? Time { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal EstimatedCost { get; set; }

        public ActivityStatus Status { get; set; } = ActivityStatus.Planned;
    }
}