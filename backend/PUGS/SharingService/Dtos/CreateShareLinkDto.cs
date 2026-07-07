using System;
using System.ComponentModel.DataAnnotations;

namespace SharingService.Dtos
{
    public class CreateShareLinkDto
    {
        [Required]
        public Guid TravelPlanId { get; set; }

        [Required]
        public string AccessLevel { get; set; } = "View"; // "View" ili "Edit"

        // Opciono - broj dana do isteka. Null = ne istice.
        public int? ExpiresInDays { get; set; }
    }
}