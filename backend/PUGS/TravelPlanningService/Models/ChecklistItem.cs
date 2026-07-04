using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelPlanningService.Models
{
    public class ChecklistItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelPlanId { get; set; }

        [ForeignKey(nameof(TravelPlanId))]
        public TravelPlan? TravelPlan { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        public bool IsCompleted { get; set; } = false;
    }
}