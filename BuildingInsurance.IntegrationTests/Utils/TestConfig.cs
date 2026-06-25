using Microsoft.Extensions.Configuration;

namespace BuildingInsurance.IntegrationTests.Utils
{
    public static class TestConfig
    {
        public static string GetTestConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.IntegrationTests.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("BuildingInsuranceDb_Test");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("ConnectionStrings:BuildingInsuranceDb_Test missing.");

            return cs;
        }
    }
}