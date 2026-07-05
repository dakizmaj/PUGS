using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BudgetService.Data
{
    public class BudgetDbContextFactory : IDesignTimeDbContextFactory<BudgetDbContext>
    {
        public BudgetDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BudgetDbContext>();
            var connectionString = configuration.GetConnectionString("BudgetDb");

            optionsBuilder.UseSqlServer(connectionString);

            return new BudgetDbContext(optionsBuilder.Options);
        }
    }
}