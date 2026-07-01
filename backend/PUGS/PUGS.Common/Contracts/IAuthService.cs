using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace PUGS.Common.Contracts
{
    public interface IAuthService : IService
    {
        Task<bool> ValidateUserExistsAsync(Guid userId);
        Task<string?> GetUserRoleAsync(Guid userId);
    }
}