using System;
using SharingService.Dtos;
using SharingService.Models;
using SharingService.Services;

namespace SharingService.Mapping
{
    public static class ShareLinkMapper
    {
        // Bazni URL frontenda - u pravom projektu ovo bi islo iz konfiguracije/.env
        private const string FrontendBaseUrl = "http://localhost:5173/shared";

        public static ShareLinkResponseDto ToResponseDto(ShareLink shareLink)
        {
            var link = $"{FrontendBaseUrl}/{shareLink.Token}";

            return new ShareLinkResponseDto
            {
                Id = shareLink.Id,
                TravelPlanId = shareLink.TravelPlanId,
                Token = shareLink.Token,
                AccessLevel = shareLink.AccessLevel.ToString(),
                ShareLink = link,
                QrCodeBase64 = QrCodeGenerator.GenerateQrCodeBase64(link),
                ExpiresAt = shareLink.ExpiresAt
            };
        }

        public static string GenerateToken()
        {
            // Kratak, URL-safe token (bez specijalnih karaktera koji prave probleme u linkovima)
            return Guid.NewGuid().ToString("N"); // 32 karaktera, samo hex
        }
    }
}