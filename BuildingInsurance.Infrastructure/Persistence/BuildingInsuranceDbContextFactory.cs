using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BuildingInsurance.Infrastructure.Persistence
{
    public class BuildingInsuranceDbContextFactory : IDesignTimeDbContextFactory<BuildingInsuranceDbContext>
    {
        public BuildingInsuranceDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(),
                "..",
                "BuildingInsurance.API"));

            var cfg = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connString = cfg.GetConnectionString("BuildingInsuranceDb");

            if (string.IsNullOrWhiteSpace(connString))
                throw new InvalidOperationException($"BuildingInsuranceDb connection string is NULL. BasePath = {basePath}");

            var opts = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlServer(connString)
                .Options;

            return new BuildingInsuranceDbContext(opts);
        }
    }
}