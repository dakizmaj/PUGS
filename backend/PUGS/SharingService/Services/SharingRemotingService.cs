using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PUGS.Common.Contracts;
using SharingService.Data;

namespace SharingService.Services
{
    public class SharingRemotingService : ISharingService
    {
        private readonly string _connectionString;

        public SharingRemotingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SharingDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SharingDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new SharingDbContext(optionsBuilder.Options);
        }

        public async Task<ShareValidationResult> ValidateTokenAsync(string token)
        {
            using var context = CreateContext();

            var shareLink = await context.ShareLinks
                .FirstOrDefaultAsync(s => s.Token == token);

            if (shareLink == null)
                return new ShareValidationResult { IsValid = false };

            if (shareLink.IsRevoked)
                return new ShareValidationResult { IsValid = false };

            if (shareLink.ExpiresAt.HasValue && shareLink.ExpiresAt.Value < DateTime.UtcNow)
                return new ShareValidationResult { IsValid = false };

            return new ShareValidationResult
            {
                IsValid = true,
                TravelPlanId = shareLink.TravelPlanId,
                AccessLevel = shareLink.AccessLevel.ToString()
            };
        }
    }
}