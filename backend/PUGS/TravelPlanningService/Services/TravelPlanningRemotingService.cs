using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PUGS.Common.Contracts;
using TravelPlanningService.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace TravelPlanningService.Services
{
    public class TravelPlanningRemotingService : ITravelPlanningService
    {
        private readonly string _connectionString;

        public TravelPlanningRemotingService(string connectionString)
        {
            _connectionString = connectionString;
        }

        private TravelPlanningDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<TravelPlanningDbContext>();
            optionsBuilder.UseSqlServer(_connectionString);
            return new TravelPlanningDbContext(optionsBuilder.Options);
        }

        public async Task<bool> PlanExistsAsync(Guid planId)
        {
            using var context = CreateContext();
            return await context.TravelPlans.AnyAsync(p => p.Id == planId);
        }

        public async Task<decimal> GetPlanBudgetAsync(Guid planId)
        {
            using var context = CreateContext();
            var plan = await context.TravelPlans.FindAsync(planId);
            return plan?.Budget ?? 0m;
        }
        public async Task<Guid?> GetPlanOwnerIdAsync(Guid planId)
        {
            using var context = CreateContext();
            var plan = await context.TravelPlans.FindAsync(planId);
            return plan?.OwnerId;
        }
        private IBudgetService GetBudgetServiceProxy()
        {
            var remotingSettings = new FabricTransportRemotingSettings { UseWrappedMessage = true };
            var clientFactory = new FabricTransportServiceRemotingClientFactory(remotingSettings);
            var proxyFactory = new ServiceProxyFactory((c) => clientFactory);

            return proxyFactory.CreateServiceProxy<IBudgetService>(
                new Uri("fabric:/PUGS.ServiceFabricApp/BudgetService"),
                new ServicePartitionKey(0),
                listenerName: "RemotingListener"
            );
        }

        public async Task DeleteAllPlansForUserAsync(Guid userId)
        {
            using var context = CreateContext();

            var plans = await context.TravelPlans
                .Where(p => p.OwnerId == userId)
                .ToListAsync();

            if (!plans.Any())
                return;

            var budgetProxy = GetBudgetServiceProxy();

            foreach (var plan in plans)
            {
                try
                {
                    await budgetProxy.DeleteAllExpensesForPlanAsync(plan.Id);
                }
                catch (Exception)
                {
                    // Budget servis nedostupan - nastavljamo brisanje plana i pored toga,
                    // troskovi ce ostati "osiroceni" ali plan ce biti obrisan kako zahteva Q&A #20
                }
            }

            context.TravelPlans.RemoveRange(plans);
            await context.SaveChangesAsync();
            // Cascade delete (vec konfigurisan u DbContext-u) automatski brise
            // povezane Destinations, Activities, ChecklistItems za svaki plan
        }
    }
}