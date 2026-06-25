using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Endpoints.Brokers
{
    [Trait("Category", "Integration")]
    [Collection("Integration")]
    public sealed class PoliciesEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public PoliciesEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateDraftPolicy_ShouldReturnCreated_AndPersist()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Policy Client",
                personalIdentificationNumber = "9900000000001",
                email = $"policy.client.{Guid.NewGuid():N}@test.com",
                phone = "0700000000",
                address = new { street = "Main", number = "1" }
            });
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClient = (await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdClient.Id);

            var createBuildingResp = await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", new
            {
                cityId,
                address = new { street = "Bld", number = "2" },
                constructionYear = 2010,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            });
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuilding = (await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdBuilding.Id);

            Guid currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = createdClient.Id,
                buildingId = createdBuilding.Id,
                currencyId,
                brokerId,
                basePremium = 1000m,
                startDate,
                endDate
            });

            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var createdPolicy = (await resp.Content.ReadFromJsonAsync<ApiResult<CreatedPolicyResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdPolicy.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == createdPolicy.Id);
                Assert.NotNull(persisted);
                Assert.Equal(createdClient.Id, persisted!.ClientId);
                Assert.Equal(createdBuilding.Id, persisted.BuildingId);
                Assert.Equal(currencyId, persisted.CurrencyId);
                Assert.Equal(brokerId, persisted.BrokerId);
                Assert.Equal(1000m, persisted.BasePremium);
            }
        }

        [Fact]
        public async Task CreateDraftPolicy_WhenClientDoesNotExist_ShouldReturnNotFound_AndNotPersist()
        {
            Guid currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = Guid.NewGuid(),
                buildingId = Guid.NewGuid(),
                currencyId,
                brokerId,
                basePremium = 1000m,
                startDate,
                endDate
            });

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

            var apiResult = await resp.Content.ReadFromJsonAsync<ApiResult<object>>(JsonOptions);
            if (apiResult is not null)
                Assert.True(apiResult.IsFailure || !apiResult.IsSuccess);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var count = await db.Policies.AsNoTracking().CountAsync(p => p.BasePremium == 1000m);
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task CreateDraftPolicy_WhenBuildingDoesNotExist_ShouldReturnNotFound_AndNotPersist()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "No Building Policy Client",
                personalIdentificationNumber = "9900000000002",
                email = $"policy.nobuilding.{Guid.NewGuid():N}@test.com",
                phone = "0700000000",
                address = new { street = "Main", number = "3" }
            });
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClient = (await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdClient.Id);

            Guid currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = createdClient.Id,
                buildingId = Guid.NewGuid(),
                currencyId,
                brokerId,
                basePremium = 777m,
                startDate,
                endDate
            });

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);

            var apiResult = await resp.Content.ReadFromJsonAsync<ApiResult<object>>(JsonOptions);
            if (apiResult is not null)
                Assert.True(apiResult.IsFailure || !apiResult.IsSuccess);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var count = await db.Policies.AsNoTracking().CountAsync(p => p.BasePremium == 777m && p.ClientId == createdClient.Id);
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task CreateDraftPolicy_WhenPremiumNegative_ShouldReturnBadRequest_AndNotPersist()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Negative Premium Client",
                personalIdentificationNumber = "9900000000003",
                email = $"policy.negprem.{Guid.NewGuid():N}@test.com",
                phone = "0700000000",
                address = new { street = "Main", number = "4" }
            });
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClient = (await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdClient.Id);

            var createBuildingResp = await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", new
            {
                cityId,
                address = new { street = "Bld", number = "5" },
                constructionYear = 2010,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            });
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuilding = (await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdBuilding.Id);

            Guid currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = createdClient.Id,
                buildingId = createdBuilding.Id,
                currencyId,
                brokerId,
                basePremium = -1m,
                startDate,
                endDate
            });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            var apiResult = await resp.Content.ReadFromJsonAsync<ApiResult<object>>(JsonOptions);
            if (apiResult is not null)
                Assert.True(apiResult.IsFailure || !apiResult.IsSuccess);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var count = await db.Policies.AsNoTracking().CountAsync(p => p.ClientId == createdClient.Id);
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task ActivatePolicy_ShouldReturnOk_AndPersistStatus()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Activate Policy Client",
                personalIdentificationNumber = "9900000000004",
                email = $"policy.activate.{Guid.NewGuid():N}@test.com",
                phone = "0700000000",
                address = new { street = "Main", number = "6" }
            });
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClient = (await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!;

            var createBuildingResp = await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", new
            {
                cityId,
                address = new { street = "Bld", number = "7" },
                constructionYear = 2010,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            });
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuilding = (await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!;

            Guid currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var createPolicyResp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = createdClient.Id,
                buildingId = createdBuilding.Id,
                currencyId,
                brokerId,
                basePremium = 1234m,
                startDate,
                endDate
            });
            Assert.Equal(HttpStatusCode.Created, createPolicyResp.StatusCode);

            var createdPolicy = (await createPolicyResp.Content.ReadFromJsonAsync<ApiResult<CreatedPolicyResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, createdPolicy.Id);

            var activateResp = await _client.PostAsync($"/api/brokers/policies/{createdPolicy.Id}/activate", null);
            Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == createdPolicy.Id);
                Assert.NotNull(persisted);

                Assert.Equal(PolicyStatus.Active, persisted!.PolicyStatus);
            }
        }

        [Fact]
        public async Task ListPolicies_WhenClientIdInvalidGuid_ShouldReturnBadRequest()
        {
            var resp = await _client.GetAsync("/api/brokers/policies?clientId=not-a-guid");
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        public void Dispose() => _client.Dispose();

        private sealed class CreatedClientResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class CreatedBuildingResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class CreatedPolicyResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class ApiResult<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get; set; }
            public string Error { get; set; } = "";
            public object? ErrorType { get; set; }
        }
    }
}