using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace PUGS.Common.Contracts
{
    public class ShareValidationResult
    {
        public bool IsValid { get; set; }
        public Guid TravelPlanId { get; set; }
        public string AccessLevel { get; set; } = string.Empty; // "View" ili "Edit"
    }

    public interface ISharingService : IService
    {
        Task<ShareValidationResult> ValidateTokenAsync(string token);
    }
}