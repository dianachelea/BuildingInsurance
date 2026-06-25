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
    public sealed class ClientsEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ClientsEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateClient_ShouldReturnCreated_AndPersist()
        {
            var email = $"john.create.{Guid.NewGuid():N}@test.com";

            var body = new
            {
                type = 1,
                fullName = "John Create",
                personalIdentificationNumber = "1234567890123",
                email = email,
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
            Assert.Equal("John Create", created.FullName);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
            var persisted = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
            Assert.NotNull(persisted);
        }

        [Fact]
        public async Task GetById_AfterCreate_ShouldReturnOk()
        {
            var email = $"john.get.{Guid.NewGuid():N}@test.com";

            var createBody = new
            {
                type = 1,
                fullName = "John Get",
                personalIdentificationNumber = "1234567890123",
                email = email,
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createResp = await _client.PostAsJsonAsync("/api/brokers/clients", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var createResult = await createResp.Content.ReadFromJsonAsync<ApiResult<ClientResponse>>(JsonOptions);
            Assert.NotNull(createResult);
            Assert.True(createResult!.IsSuccess);

            var created = createResult.Value;
            Assert.NotNull(created);

            var getResp = await _client.GetAsync($"/api/brokers/clients/{created!.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            var getResult = await getResp.Content.ReadFromJsonAsync<ApiResult<ClientResponse>>(JsonOptions);
            Assert.NotNull(getResult);
            Assert.True(getResult!.IsSuccess);

            var details = getResult.Value;
            Assert.NotNull(details);
            Assert.Equal(created.Id, details!.Id);
            Assert.Equal("John Get", details.FullName);
        }

        [Fact]
        public async Task GetById_WhenInvalidGuid_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync("/api/brokers/clients/not-a-guid");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ShouldReturnNotFound()
        {
            var resp = await _client.GetAsync($"/api/brokers/clients/{Guid.NewGuid()}");
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateClient_WhenInvalidGuid_ShouldReturnNotFound()
        {
            var body = new
            {
                fullName = "Updated Name",
                email = "updated@test.com",
                phone = "0711111111",
                address = new { street = "Updated", number = "1" },
                identificationNumber = "1234567890123",
                identificationChangeReason = "Typo correction"
            };

            var resp = await _client.PutAsJsonAsync("/api/brokers/clients/not-a-guid", body);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateClient_WhenNotFound_ShouldReturnNotFound()
        {
            var body = new
            {
                fullName = "Updated Name",
                email = $"updated.{Guid.NewGuid():N}@test.com",
                phone = "0711111111",
                address = new { street = "Updated", number = "1" },
                identificationNumber = "1234567890123",
                identificationChangeReason = "Typo correction"
            };

            var resp = await _client.PutAsJsonAsync($"/api/brokers/clients/{Guid.NewGuid()}", body);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task UpdateClient_AfterCreate_ShouldReturnOk_AndPersistChanges()
        {
            var email = $"john.upd.{Guid.NewGuid():N}@test.com";

            var createBody = new
            {
                type = 1,
                fullName = "John ToUpdate",
                personalIdentificationNumber = "1234567890123",
                email = email,
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createResp = await _client.PostAsJsonAsync("/api/brokers/clients", createBody);
            Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

            var createResult = await createResp.Content.ReadFromJsonAsync<ApiResult<ClientResponse>>(JsonOptions);
            Assert.NotNull(createResult);
            Assert.True(createResult!.IsSuccess);

            var created = createResult.Value;
            Assert.NotNull(created);

            var updateBody = new
            {
                fullName = "John Updated",
                email = $"john.updated.{Guid.NewGuid():N}@test.com",
                phone = "0799999999",
                address = new { street = "Updated St", number = "22" },
                identificationNumber = "1234567890123",
                identificationChangeReason = "Legal entity update"
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/brokers/clients/{created!.Id}", updateBody);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            var updateResult = await updateResp.Content.ReadFromJsonAsync<ApiResult<ClientResponse>>(JsonOptions);
            Assert.NotNull(updateResult);
            Assert.True(updateResult!.IsSuccess);

            var updated = updateResult.Value;
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated!.Id);
            Assert.Equal("John Updated", updated.FullName);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
            var persisted = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == created.Id);
            Assert.NotNull(persisted);
        }

        [Fact]
        public async Task ListClients_WhenEmpty_ShouldReturnOk_WithEmptyItems()
        {
            var resp = await _client.GetAsync("/api/brokers/clients?page=1&pageSize=20");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListClientsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var list = result.Value;
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        [Fact]
        public async Task ListClients_WithNameFilter_ShouldReturnOnlyMatches()
        {
            var email1 = $"ana.{Guid.NewGuid():N}@test.com";
            var email2 = $"bob.{Guid.NewGuid():N}@test.com";

            var c1 = new
            {
                type = 1,
                fullName = "Ana Maria",
                personalIdentificationNumber = "1234567890023",
                email = email1,
                phone = "0700000000",
                address = new { street = "Main", number = "1" }
            };

            var c2 = new
            {
                type = 1,
                fullName = "Bob Pop",
                personalIdentificationNumber = "1234567890123",
                email = email2,
                phone = "0700000000",
                address = new { street = "Main", number = "1" }
            };

            var r1 = await _client.PostAsJsonAsync("/api/brokers/clients", c1);
            Assert.Equal(HttpStatusCode.Created, r1.StatusCode);

            var r2 = await _client.PostAsJsonAsync("/api/brokers/clients", c2);
            Assert.Equal(HttpStatusCode.Created, r2.StatusCode);

            var resp = await _client.GetAsync("/api/brokers/clients?name=Ana&page=1&pageSize=20");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ApiResult<ListClientsResponse>>(JsonOptions);
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var list = result.Value;
            Assert.NotNull(list);

            Assert.True(list!.TotalCount >= 1);
            Assert.All(list.Items, x => Assert.Contains("Ana", x.FullName));
        }

        [Fact]
        public async Task ListClients_Paging_ShouldReturnDifferentPages()
        {
            for (int i = 1; i <= 3; i++)
            {
                var body = new
                {
                    type = 1,
                    fullName = $"Paged Client {i}",
                    personalIdentificationNumber = $"{i}234567890123",
                    email = $"paged.{i}.{Guid.NewGuid():N}@test.com",
                    phone = "070000000",
                    address = new { street = "Main", number = "1" }
                };

                var resp = await _client.PostAsJsonAsync("/api/brokers/clients", body);
                Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
            }

            var page1Resp = await _client.GetAsync("/api/brokers/clients?page=1&pageSize=2");
            Assert.Equal(HttpStatusCode.OK, page1Resp.StatusCode);

            var page1Result = await page1Resp.Content.ReadFromJsonAsync<ApiResult<ListClientsResponse>>(JsonOptions);
            Assert.NotNull(page1Result);
            Assert.True(page1Result!.IsSuccess);

            var page1 = page1Result.Value!;
            Assert.True(page1.Items.Count <= 2);

            var page2Resp = await _client.GetAsync("/api/brokers/clients?page=2&pageSize=2");
            Assert.Equal(HttpStatusCode.OK, page2Resp.StatusCode);

            var page2Result = await page2Resp.Content.ReadFromJsonAsync<ApiResult<ListClientsResponse>>(JsonOptions);
            Assert.NotNull(page2Result);
            Assert.True(page2Result!.IsSuccess);

            var page2 = page2Result.Value!;
            Assert.True(page2.Items.Count <= 2);

            if (page1.Items.Count > 0 && page2.Items.Count > 0)
            {
                Assert.DoesNotContain(page2.Items[0].Id, page1.Items.Select(x => x.Id));
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private sealed class ApiResult<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get; set; }
            public string Error { get; set; } = "";
            public string ErrorType { get; set; } = "";
        }

        private sealed class ClientResponse
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = "";
        }

        private sealed class ListClientsResponse
        {
            public List<ClientItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class ClientItem
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = "";
        }
    }
}