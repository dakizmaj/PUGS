using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Data
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            var connectionString = configuration.GetConnectionString("AuthDb");

            optionsBuilder.UseSqlServer(connectionString);

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}