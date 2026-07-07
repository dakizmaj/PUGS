using System;

namespace SharingService.Dtos
{
    public class ShareLinkResponseDto
    {
        public Guid Id { get; set; }
        public Guid TravelPlanId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string ShareLink { get; set; } = string.Empty;
        public string QrCodeBase64 { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}