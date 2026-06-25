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
    public sealed class CurrenciesEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CurrenciesEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateCurrency_ShouldReturnCreated_AndPersist()
        {
            var body = new
            {
                code = "USD",
                name = "US Dollar",
                exchangeRateToBase = 4.50m,
                isActive = true
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/currencies", body);
            Assert.Equal(HttpStatusCode.Created, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<CurrencyResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var created = result.Value;
            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created!.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal("US Dollar", persisted!.Name);
                Assert.True(persisted.IsActive);
            }
        }

        [Fact]
        public async Task CreateCurrency_WhenValidationFails_ShouldReturnBadRequest()
        {
            var body = new
            {
                code = "USD",
                name = "",
                exchangeRateToBase = 0m,
                isActive = true
            };

            var resp = await _client.PostAsJsonAsync("/api/admin/currencies", body);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task GetCurrencyById_ShouldReturnOk_WhenExists()
        {
            var createResp = await _client.PostAsJsonAsync("/api/admin/currencies", new
            {
                code = "RON",
                name = "Lei",
                exchangeRateToBase = 1m,
                isActive = true
            });
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<CurrencyResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var resp = await _client.GetAsync($"/api/admin/currencies/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<CurrencyDetailsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            Assert.NotNull(result.Value);
            Assert.Equal(created.Id, result.Value!.Id);
            Assert.Equal("Lei", result.Value.Name);
        }

        [Fact]
        public async Task GetCurrencyById_WhenNotFound_ShouldReturnNotFound()
        {
            var missingId = Guid.NewGuid();

            var resp = await _client.GetAsync($"/api/admin/currencies/{missingId}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateCurrency_ShouldReturnOk_AndPersistChanges()
        {
            var createResp = await _client.PostAsJsonAsync("/api/admin/currencies", new
            {
                code = "USD",
                name = "Usd Dollar",
                exchangeRateToBase = 5.20m,
                isActive = true
            });
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var created = (await createResp.Content.ReadFromJsonAsync<ApiResult<CurrencyResponse>>(JsonOptions))!.Value!;
            Assert.NotEqual(Guid.Empty, created.Id);

            var updateBody = new
            {
                name = "Usd Dollar Updated",
                exchangeRateToBase = 5.55m,
                isActive = false
            };

            var routeGuid = Guid.NewGuid();
            var updateResp = await _client.PutAsJsonAsync($"/api/admin/currencies/{created.Id}", updateBody);

            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var persisted = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
                Assert.NotNull(persisted);
                Assert.Equal("Usd Dollar Updated", persisted!.Name);
                Assert.Equal(5.55m, persisted.ExchangeRateToBase);
                Assert.False(persisted.IsActive);
            }
        }

        [Fact]
        public async Task ListCurrencies_ShouldReturnOk()
        {
            var resp = await _client.GetAsync("/api/admin/currencies?page=1&pageSize=5");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListCurrenciesResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);
            Assert.NotNull(result.Value);
        }

        public void Dispose() => _client.Dispose();

        private sealed class CurrencyResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class CurrencyDetailsResponse
        {
            public Guid Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public decimal ExchangeRateToBase { get; set; }
            public bool IsActive { get; set; }
        }

        private sealed class ListCurrenciesResponse
        {
            public List<CurrencyItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class CurrencyItem
        {
            public Guid Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public bool IsActive { get; set; }
            public decimal ExchangeRateToBase { get; set; }
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