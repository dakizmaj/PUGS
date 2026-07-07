using System;
using System.ComponentModel.DataAnnotations;

namespace SharingService.Models
{
    public enum ShareAccessLevel
    {
        View,
        Edit
    }

    public class ShareLink
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid TravelPlanId { get; set; }

        // Token koji ide u link/QR kod - kratak, jedinstven string
        [Required, MaxLength(64)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public ShareAccessLevel AccessLevel { get; set; }

        // Korisnik koji je kreirao share link (vlasnik plana ili admin)
        [Required]
        public Guid CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Opciono: isticanje linka. Null = ne istice nikad.
        public DateTime? ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}