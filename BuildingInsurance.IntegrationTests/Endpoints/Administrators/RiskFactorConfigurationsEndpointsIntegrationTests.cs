using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BuildingInsurance.IntegrationTests.Endpoints.Administrators
{
    [Trait("Category", "Integration")]
    [Collection("Integration")]
    public sealed class RiskFactorConfigurationsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RiskFactorConfigurationsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_ShouldReturnCreated_AndPersist()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(x => x.Id).FirstAsync();
            }

            var body = new
            {
                level = 3,
                referenceId = cityId,
                buildingType = (int?)null,
                adjustmentPercentage = 0.20m,
                isActive = true
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/risk-factors", body);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<CreatedRiskFactorResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.RiskFactorConfigurations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == created.Id);
                Assert.NotNull(persisted);

                Assert.Equal(cityId, persisted!.ReferenceId);
                Assert.True(persisted.IsActive);
                Assert.Equal(0.20m, persisted.AdjustmentPercentage);
            }
        }

        [Fact]
        public async Task CreateRiskFactorConfiguration_WhenValidationFails_ShouldReturnBadRequest()
        {
            var body = new
            {
                level = 3,
                referenceId = Guid.NewGuid(),
                buildingType = (int?)null,
                adjustmentPercentage = 2.0m,
                isActive = true
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/risk-factors", body);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_ShouldReturnOk_WhenExists()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(x => x.Id).FirstAsync();
            }

            var createResp = await _client.PostAsJsonAsync("/api/admin/risk-factors", new
            {
                level = 3,
                referenceId = cityId,
                buildingType = (int?)null,
                adjustmentPercentage = -0.10m,
                isActive = true
            });

            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<CreatedRiskFactorResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var getResp = await _client.GetAsync($"/api/admin/risk-factors/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            var getResult = await getResp.Content.ReadFromJsonAsync<ApiResult<RiskFactorDetailsResponse>>(JsonOptions);
            Assert.NotNull(getResult);
            Assert.True(getResult!.IsSuccess);

            Assert.NotNull(getResult.Value);
            Assert.Equal(created.Id, getResult.Value!.Id);
            Assert.Equal(cityId, getResult.Value.ReferenceId);
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync($"/api/admin/risk-factors/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetRiskFactorConfigurationById_WhenIdInvalidGuid_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync("/api/admin/risk-factors/not-a-guid");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_ShouldReturnOk_AndPersistChanges()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(x => x.Id).FirstAsync();
            }

            var createResp = await _client.PostAsJsonAsync("/api/admin/risk-factors", new
            {
                level = 3,
                referenceId = cityId,
                buildingType = (int?)null,
                adjustmentPercentage = 0.05m,
                isActive = true
            });

            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<CreatedRiskFactorResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var updateBody = new
            {
                level = 3,
                referenceId = cityId,
                buildingType = (int?)null,
                adjustmentPercentage = 0.15m,
                isActive = false
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/admin/risk-factors/{created.Id}", updateBody);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.RiskFactorConfigurations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == created.Id);

                Assert.NotNull(persisted);
                Assert.Equal(cityId, persisted!.ReferenceId);
                Assert.Equal(0.15m, persisted.AdjustmentPercentage);
                Assert.False(persisted.IsActive);
            }
        }

        [Fact]
        public async Task UpdateRiskFactorConfiguration_WhenIdInvalidGuid_ShouldReturnNotFound()
        {
            var updateBody = new
            {
                level = 3,
                referenceId = Guid.NewGuid(),
                buildingType = (int?)null,
                adjustmentPercentage = 0.15m,
                isActive = false
            };

            var resp = await _client.PutAsJsonAsync("/api/admin/risk-factors/not-a-guid", updateBody);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_ShouldReturnOk()
        {
            var resp = await _client.GetAsync("/api/admin/risk-factors?page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListRiskFactorsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task ListRiskFactorConfigurations_FilterByLevel_ShouldReturnOk()
        {
            var resp = await _client.GetAsync("/api/admin/risk-factors?level=3&page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListRiskFactorsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);
        }

        public void Dispose() => _client.Dispose();

        private sealed class CreatedRiskFactorResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class RiskFactorDetailsResponse
        {
            public Guid Id { get; set; }
            public string Level { get; set; } = "";
            public Guid ReferenceId { get; set; }
            public string BuildingType { get; set; } = "";
            public decimal AdjustmentPercentage { get; set; }
            public bool IsActive { get; set; }
        }

        private sealed class ListRiskFactorsResponse
        {
            public List<RiskFactorItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class RiskFactorItem
        {
            public Guid Id { get; set; }
            public string Level { get; set; } = "";
            public Guid ReferenceId { get; set; }
            public decimal AdjustmentPercentage { get; set; }
            public bool IsActive { get; set; }
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