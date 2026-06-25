using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Endpoints.Brokers
{
    [Trait("Category", "Integration")]
    [Collection("Integration")]
    public sealed class BuildingsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BuildingsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();
            _client = factory.CreateClient();
        }

        private async Task<Guid> CreateClientAsync(string fullName, string email)
        {
            var body = new
            {
                type = 1,
                fullName,
                personalIdentificationNumber = "1234567890123",
                email,
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var resp = await _client.PostAsJsonAsync("/api/brokers/clients", body);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ClientResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            return created.Id;
        }

        private async Task<Guid> GetAnyCityIdAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
            return await db.Cities.Select(c => c.Id).FirstAsync();
        }

        [Fact]
        public async Task CreateBuilding_ShouldReturnCreated_AndPersist()
        {
            var clientId = await CreateClientAsync("Building Client", $"building.client.{Guid.NewGuid():N}@test.com");
            var cityId = await GetAnyCityIdAsync();

            var createBody = new
            {
                cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120m,
                insuredValue = 150000m,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", createBody);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal(clientId, persisted!.ClientId);
            }
        }

        [Fact]
        public async Task CreateBuilding_WhenClientIdInvalidGuid_ShouldReturnNotFound()
        {
            var cityId = await GetAnyCityIdAsync();

            var createBody = new
            {
                cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120m,
                insuredValue = 150000m,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync("/api/brokers/clients/not-a-guid/buildings", createBody);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task CreateBuilding_WhenClientNotFound_ShouldReturnNotFound()
        {
            var cityId = await GetAnyCityIdAsync();

            var createBody = new
            {
                cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120m,
                insuredValue = 150000m,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync($"/api/brokers/clients/{Guid.NewGuid()}/buildings", createBody);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task CreateBuilding_WhenValidationFails_ShouldReturnBadRequest()
        {
            var clientId = await CreateClientAsync("Invalid Building Client", $"invalid.building.{Guid.NewGuid():N}@test.com");
            var cityId = await GetAnyCityIdAsync();

            var createBody = new
            {
                cityId,
                address = new { street = "", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120m,
                insuredValue = 150000m,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", createBody);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        public void Dispose() => _client.Dispose();

        private sealed class ClientResponse
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = "";
        }

        private sealed class CreatedBuildingResponse
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public Guid CityId { get; set; }
            public string Street { get; set; } = "";
            public string Number { get; set; } = "";
        }

        private sealed class ApiResult<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get; set; }
            public string Error { get; set; } = "";
            public string ErrorType { get; set; } = "";
        }
    }
}