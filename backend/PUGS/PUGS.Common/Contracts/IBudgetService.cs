using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace PUGS.Common.Contracts
{
    public interface IBudgetService : IService
    {
        Task RecordActivityExpenseAsync(Guid travelPlanId, Guid activityId, string activityName, decimal estimatedCost, DateTime activityDate);
        Task UpdateActivityExpenseAsync(Guid activityId, string activityName, decimal newEstimatedCost, DateTime activityDate);
        Task RemoveActivityExpenseAsync(Guid activityId);
    }
}