using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace PUGS.Common.Contracts
{
    public interface ITravelPlanningService : IService
    {
        Task<bool> PlanExistsAsync(Guid planId);
        Task<decimal> GetPlanBudgetAsync(Guid planId);
        Task<Guid?> GetPlanOwnerIdAsync(Guid planId);
    }
}