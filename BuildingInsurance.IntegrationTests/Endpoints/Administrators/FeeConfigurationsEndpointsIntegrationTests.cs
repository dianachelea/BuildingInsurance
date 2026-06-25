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
    public sealed class FeeConfigurationsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public FeeConfigurationsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateFeeConfiguration_ShouldReturnCreated_AndPersist()
        {
            var body = new
            {
                name = $"Admin Fee {Guid.NewGuid():N}",
                feeType = 1,
                feePercentage = 0.10m,
                effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = true,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/fees", body);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<FeeConfigCreatedResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.FeeConfigurations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal(body.name, persisted!.Name);
                Assert.True(persisted.IsActive);
            }
        }

        [Fact]
        public async Task CreateFeeConfiguration_WhenValidationFails_ShouldReturnBadRequest()
        {
            var body = new
            {
                name = "",
                feeType = 1,
                feePercentage = 0m,
                effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = true,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/fees", body);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetFeeConfigurationById_ShouldReturnOk_WhenExists()
        {
            var createResp = await _client.PostAsJsonAsync("/api/admin/fees", new
            {
                name = $"Get Fee {Guid.NewGuid():N}",
                feeType = 1,
                feePercentage = 0.12m,
                effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = true,
                riskIndicators = 0
            });

            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<FeeConfigCreatedResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var getResp = await _client.GetAsync($"/api/admin/fees/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            var getResult = await getResp.Content.ReadFromJsonAsync<ApiResult<FeeConfigurationDetailsResponse>>(JsonOptions);
            Assert.NotNull(getResult);
            Assert.True(getResult!.IsSuccess);

            Assert.NotNull(getResult.Value);
            Assert.Equal(created.Id, getResult.Value!.Id);
        }

        [Fact]
        public async Task GetFeeConfigurationById_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync($"/api/admin/fees/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetFeeConfigurationById_WhenIdInvalidGuid_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync("/api/admin/fees/not-a-guid");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateFeeConfiguration_ShouldReturnOk_AndPersistChanges()
        {
            var createResp = await _client.PostAsJsonAsync("/api/admin/fees", new
            {
                name = $"Update Fee {Guid.NewGuid():N}",
                feeType = 1,
                feePercentage = 0.10m,
                effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = true,
                riskIndicators = 0
            });

            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<FeeConfigCreatedResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var updateBody = new
            {
                name = "Updated Fee Name",
                feetype = 1,
                feepercentage = 0.15m,
                effectiveFrom = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = false,
                riskIndicators = 0
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/admin/fees/{created.Id}", updateBody);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.FeeConfigurations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == created.Id);

                Assert.NotNull(persisted);
                Assert.Equal("Updated Fee Name", persisted!.Name);
                Assert.False(persisted.IsActive);
                Assert.Equal(0.15m, persisted.FeePercentage);
            }
        }

        [Fact]
        public async Task UpdateFeeConfiguration_WhenIdInvalidGuid_ShouldReturnNotFound()
        {
            var updateBody = new
            {
                name = "Updated Fee Name",
                type = 1,
                percentage = 0.15m,
                effectiveFrom = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                effectiveTo = new DateTime(2026, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                isActive = false,
                riskIndicators = 0
            };

            var resp = await _client.PutAsJsonAsync("/api/admin/fees/not-a-guid", updateBody);

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task ListFeeConfigurations_ShouldReturnOk()
        {
            var resp = await _client.GetAsync("/api/admin/fees?page=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListFeeConfigurationsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);
        }

        public void Dispose() => _client.Dispose();

        private sealed class FeeConfigCreatedResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class FeeConfigurationDetailsResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public decimal FeePercentage { get; set; }
            public bool IsActive { get; set; }
        }

        private sealed class ListFeeConfigurationsResponse
        {
            public List<FeeConfigurationItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class FeeConfigurationItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public string Type { get; set; } = "";
            public decimal FeePercentage { get; set; }
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