using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PUGS.Common.Contracts;
using TravelPlanningService.Data;

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
    }
}