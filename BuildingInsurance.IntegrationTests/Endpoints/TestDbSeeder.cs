using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.IntegrationTests.Endpoints
{
    public static class TestDbSeeder
    {
        public static async Task ResetAndSeedAsync(string connectionString)
        {
            var options = new DbContextOptionsBuilder<BuildingInsuranceDbContext>()
                .UseSqlServer(connectionString, sql => sql.MigrationsAssembly("BuildingInsurance.Infrastructure"))
                .Options;

            await using var db = new BuildingInsuranceDbContext(options);

            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();

            var country = new Domain.Entities.Geography.Country(Guid.NewGuid(), "Romania");
            db.Countries.Add(country);

            var county = new Domain.Entities.Geography.County(Guid.NewGuid(), "Cluj", country.Id);
            db.Counties.Add(county);

            var city = new Domain.Entities.Geography.City(Guid.NewGuid(), "Cluj-Napoca", county.Id);
            db.Cities.Add(city);

            var eur = new Currency(id: Guid.NewGuid(), code: "EUR", name: "Euro", exchangeRateToBase: 1m, isActive: true);
            db.Currencies.Add(eur);

            var contactInfo = new ContactInfo(email: "broker.integration@test.com", phone: "0700000000");

            var broker = new Domain.Entities.Management.Broker(
                id: Guid.NewGuid(),
                brokerCode: "INT-001",
                name: "Integration Broker",
                contactInfo: contactInfo,
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            db.Brokers.Add(broker);

            await db.SaveChangesAsync();
        }
    }
}