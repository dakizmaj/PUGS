using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace TravelPlanningService.Models
{
    public class TravelPlan
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Vlasnik plana - referenca na User.Id iz Auth servisa (bez FK jer je druga baza)
        [Required]
        public Guid OwnerId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Budget { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigacione osobine
        public List<Destination> Destinations { get; set; } = new();
        public List<Activity> Activities { get; set; } = new();
        public List<ChecklistItem> ChecklistItems { get; set; } = new();
    }
}