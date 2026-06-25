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
    public sealed class BrokersEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BrokersEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateBroker_ShouldReturnCreated_AndPersist()
        {
            var email = $"broker.{Guid.NewGuid():N}@test.com";
            var body = new
            {
                brokerCode = $"BR-{Guid.NewGuid():N}".Substring(0, 10),
                fullName = "Broker One",
                email,
                phone = "0700000000",
                commissionPercentage = 0.10m
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/brokers", body);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<BrokerResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal(body.fullName, persisted!.FullName);
            }
        }

        [Fact]
        public async Task GetBrokerById_ShouldReturnOk_WhenExists()
        {
            var email = $"getbroker.{Guid.NewGuid():N}@test.com";
            var createBody = new
            {
                brokerCode = $"GB-{Guid.NewGuid():N}".Substring(0, 10),
                fullName = "Get Broker",
                email,
                phone = "0700000000",
                commissionPercentage = 0.10m
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/brokers", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<BrokerResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var resp = await _client.GetAsync($"/api/admin/brokers/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<BrokerDetailsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            Assert.NotNull(result.Value);
            Assert.Equal(created.Id, result.Value!.Id);
        }

        [Fact]
        public async Task GetBrokerById_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync($"/api/admin/brokers/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task ActivateBroker_ShouldReturnOk_AndSetActive()
        {
            var email = $"activate.{Guid.NewGuid():N}@test.com";
            var createBody = new
            {
                brokerCode = $"AC-{Guid.NewGuid():N}".Substring(0, 10),
                fullName = "Activate Broker",
                email,
                phone = "0700000000",
                commissionPercentage = 0.10m
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/brokers", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<BrokerResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var deactivateResp = await _client.PostAsync($"/api/admin/brokers/{created.Id}/deactivate", null);
            Assert.True(deactivateResp.StatusCode == HttpStatusCode.OK);

            var activateResp = await _client.PostAsync($"/api/admin/brokers/{created.Id}/activate", null);
            Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal(Domain.Enums.BrokerStatus.Active, persisted!.BrokerStatus);
            }
        }

        [Fact]
        public async Task DeactivateBroker_ShouldReturnOk_AndSetInactive()
        {
            var email = $"deactivate.{Guid.NewGuid():N}@test.com";
            var createBody = new
            {
                brokerCode = $"DC-{Guid.NewGuid():N}".Substring(0, 10),
                fullName = "Deactivate Broker",
                email,
                phone = "0700000000",
                commissionPercentage = 0.10m
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/brokers", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<BrokerResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var deactivateResp = await _client.PostAsync($"/api/admin/brokers/{created.Id}/deactivate", null);
            Assert.Equal(HttpStatusCode.OK, deactivateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal(Domain.Enums.BrokerStatus.Inactive, persisted!.BrokerStatus);
            }
        }

        [Fact]
        public async Task UpdateBroker_ShouldReturnOk_AndPersistChanges()
        {
            var email = $"update.{Guid.NewGuid():N}@test.com";
            var createBody = new
            {
                brokerCode = $"UP-{Guid.NewGuid():N}".Substring(0, 10),
                fullName = "Update Broker",
                email,
                phone = "0700000000",
                commissionPercentage = 0.10m
            };

            var createResp = await _client.PostAsJsonAsync("/api/admin/brokers", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<BrokerResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var updateBody = new
            {
                fullName = "Updated Broker Name",
                email = $"updated.{Guid.NewGuid():N}@test.com",
                phone = "0799999999",
                commissionPercentage = 0.15m
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/admin/brokers/{created.Id}", updateBody);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal("Updated Broker Name", persisted!.FullName);
                Assert.Equal(updateBody.phone, persisted.ContactInfo.Phone);
                Assert.Equal(updateBody.email, persisted.ContactInfo.Email);
                Assert.Equal(updateBody.commissionPercentage, persisted.CommissionPercentage);
            }
        }

        [Fact]
        public async Task ListBrokers_ShouldReturnOk()
        {
            var resp = await _client.GetAsync("/api/admin/brokers?page=1&pageSize=15");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListBrokersResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value!.TotalCount >= 1);
        }

        [Fact]
        public async Task ActivateBroker_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.PostAsync($"/api/admin/brokers/{Guid.NewGuid()}/activate", null);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task DeactivateBroker_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.PostAsync($"/api/admin/brokers/{Guid.NewGuid()}/deactivate", null);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        public void Dispose() => _client.Dispose();

        private sealed class BrokerResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class BrokerDetailsResponse
        {
            public Guid Id { get; set; }
            public string BrokerCode { get; set; } = "";
            public string FullName { get; set; } = "";
            public bool IsActive { get; set; }
            public string Email { get; set; } = "";
            public string Phone { get; set; } = "";
            public decimal? CommissionPercentage { get; set; }
        }

        private sealed class ListBrokersResponse
        {
            public List<BrokerItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class BrokerItem
        {
            public Guid Id { get; set; }
            public string BrokerCode { get; set; } = "";
            public string FullName { get; set; } = "";
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