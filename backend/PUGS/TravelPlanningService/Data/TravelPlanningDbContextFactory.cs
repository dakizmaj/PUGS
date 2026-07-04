using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TravelPlanningService.Data
{
    public class TravelPlanningDbContextFactory : IDesignTimeDbContextFactory<TravelPlanningDbContext>
    {
        public TravelPlanningDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<TravelPlanningDbContext>();
            var connectionString = configuration.GetConnectionString("TravelPlanningDb");

            optionsBuilder.UseSqlServer(connectionString);

            return new TravelPlanningDbContext(optionsBuilder.Options);
        }
    }
}