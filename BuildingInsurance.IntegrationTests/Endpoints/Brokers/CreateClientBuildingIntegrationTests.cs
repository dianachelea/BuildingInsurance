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
    public sealed class CreateClientBuildingIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public CreateClientBuildingIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateClient_ThenCreateBuilding_ThenGetUpdateList_ShouldWork_EndToEnd()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientBody = new
            {
                type = 1,
                fullName = "John Api",
                personalIdentificationNumber = "1234567890123",
                email = "john.api@test.com",
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClientBody);
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);
            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);

            var createdClient = createdClientResult.Value;
            Assert.NotNull(createdClient);
            Assert.NotEqual(Guid.Empty, createdClient!.Id);

            var createBuildingBody = new
            {
                cityId = cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            };

            var createBuildingResp =
                await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", createBuildingBody);
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuildingResult =
                await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(createdBuildingResult);
            Assert.True(createdBuildingResult!.IsSuccess);

            var createdBuilding = createdBuildingResult.Value;
            Assert.NotNull(createdBuilding);
            Assert.NotEqual(Guid.Empty, createdBuilding!.Id);
            Assert.Equal(createdClient.Id, createdBuilding.ClientId);
            Assert.Equal(cityId, createdBuilding.CityId);

            var getBuildingResp = await _client.GetAsync($"/api/brokers/buildings/{createdBuilding.Id}");
            Assert.Equal(HttpStatusCode.OK, getBuildingResp.StatusCode);

            var buildingDetailsResult =
                await getBuildingResp.Content.ReadFromJsonAsync<ApiResult<BuildingDetailsResponse>>(JsonOptions);
            Assert.NotNull(buildingDetailsResult);
            Assert.True(buildingDetailsResult!.IsSuccess);

            var buildingDetails = buildingDetailsResult.Value;
            Assert.NotNull(buildingDetails);
            Assert.Equal(createdBuilding.Id, buildingDetails!.Id);
            Assert.Equal(createdClient.Id, buildingDetails.ClientId);

            var updateBody = new
            {
                cityId = cityId,
                address = new { street = "Updated St", number = "22" },
                constructionYear = 2005,
                type = 2,
                numberOfFloors = 3,
                surfaceArea = 200,
                insuredValue = 250000,
                riskIndicators = 0
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/brokers/buildings/{createdBuilding.Id}", updateBody);
            Assert.Equal(HttpStatusCode.OK, updateResp.StatusCode);

            var updatedResult =
                await updateResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(updatedResult);
            Assert.True(updatedResult!.IsSuccess);

            var updated = updatedResult.Value;
            Assert.NotNull(updated);
            Assert.Equal(createdBuilding.Id, updated!.Id);

            var listResp = await _client.GetAsync($"/api/brokers/clients/{createdClient.Id}/buildings");
            Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

            var listResult =
                await listResp.Content.ReadFromJsonAsync<ApiResult<ListBuildingsResponse>>(JsonOptions);
            Assert.NotNull(listResult);
            Assert.True(listResult!.IsSuccess);

            var list = listResult.Value;
            Assert.NotNull(list);
            Assert.True(list!.TotalCount >= 1);
            Assert.Contains(list.Items, x => x.Id == createdBuilding.Id);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var persistedClient = await db.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == createdClient.Id);
                Assert.NotNull(persistedClient);

                var persistedBuilding = await db.Buildings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == createdBuilding.Id);
                Assert.NotNull(persistedBuilding);
                Assert.Equal(createdClient.Id, persistedBuilding!.ClientId);
            }
        }

        [Fact]
        public async Task CreateClient_ThenListBuildings_ShouldReturnEmptyList()
        {
            var createClientBody = new
            {
                type = 1,
                fullName = "John Api Empty",
                personalIdentificationNumber = "1234567890123",
                email = "john.api.empty@test.com",
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClientBody);
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);
            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);

            var createdClient = createdClientResult.Value;
            Assert.NotNull(createdClient);
            Assert.NotEqual(Guid.Empty, createdClient!.Id);

            var listResp = await _client.GetAsync($"/api/brokers/clients/{createdClient.Id}/buildings");
            Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

            var listResult =
                await listResp.Content.ReadFromJsonAsync<ApiResult<ListBuildingsResponse>>(JsonOptions);
            Assert.NotNull(listResult);
            Assert.True(listResult!.IsSuccess);

            var list = listResult.Value;
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        [Fact]
        public async Task CreateBuilding_WhenClientDoesNotExist_ShouldReturnNotFound()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createBuildingBody = new
            {
                cityId = cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            };

            var resp = await _client.PostAsJsonAsync($"/api/brokers/clients/{Guid.NewGuid()}/buildings", createBuildingBody);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task CreateClient_ThenCreateBuilding_WhenInvalid_ShouldReturnBadRequest_AndNotPersist()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientBody = new
            {
                type = 1,
                fullName = "John Api 400",
                personalIdentificationNumber = "1234567890123",
                email = "john.api.400@test.com",
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClientBody);
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);
            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);

            var createdClient = createdClientResult.Value;
            Assert.NotNull(createdClient);
            Assert.NotEqual(Guid.Empty, createdClient!.Id);

            var invalidBuildingBody = new
            {
                cityId = cityId,
                address = new { street = "", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            };

            var createBuildingResp =
                await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", invalidBuildingBody);

            Assert.Equal(HttpStatusCode.BadRequest, createBuildingResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var buildingsCount = await db.Buildings.AsNoTracking().CountAsync(b => b.ClientId == createdClient.Id);
                Assert.Equal(0, buildingsCount);
            }
        }

        [Fact]
        public async Task CreateClient_ThenCreateTwoBuildings_ThenList_ShouldContainBoth()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientBody = new
            {
                type = 1,
                fullName = "John Api 2B",
                personalIdentificationNumber = "1234567890123",
                email = "john.api.2b@test.com",
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClientBody);
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);
            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);

            var createdClient = createdClientResult.Value;
            Assert.NotNull(createdClient);
            Assert.NotEqual(Guid.Empty, createdClient!.Id);

            var building1 = new
            {
                cityId = cityId,
                address = new { street = "Street 1", number = "1" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            };

            var building2 = new
            {
                cityId = cityId,
                address = new { street = "Street 2", number = "2" },
                constructionYear = 2010,
                type = 1,
                numberOfFloors = 1,
                surfaceArea = 80,
                insuredValue = 90000,
                riskIndicators = 0
            };

            var resp1 = await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", building1);
            Assert.Equal(HttpStatusCode.Created, resp1.StatusCode);

            var created1Result =
                await resp1.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(created1Result);
            Assert.True(created1Result!.IsSuccess);

            var created1 = created1Result.Value;
            Assert.NotNull(created1);

            var resp2 = await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient.Id}/buildings", building2);
            Assert.Equal(HttpStatusCode.Created, resp2.StatusCode);

            var created2Result =
                await resp2.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(created2Result);
            Assert.True(created2Result!.IsSuccess);

            var created2 = created2Result.Value;
            Assert.NotNull(created2);

            var listResp = await _client.GetAsync($"/api/brokers/clients/{createdClient.Id}/buildings");
            Assert.Equal(HttpStatusCode.OK, listResp.StatusCode);

            var listResult =
                await listResp.Content.ReadFromJsonAsync<ApiResult<ListBuildingsResponse>>(JsonOptions);
            Assert.NotNull(listResult);
            Assert.True(listResult!.IsSuccess);

            var list = listResult.Value;
            Assert.NotNull(list);

            Assert.True(list!.TotalCount >= 2);
            Assert.Contains(list.Items, x => x.Id == created1!.Id);
            Assert.Contains(list.Items, x => x.Id == created2!.Id);
        }

        [Fact]
        public async Task CreateClient_ThenCreateBuilding_ThenUpdateInvalid_ShouldReturnBadRequest_AndNotChangeBuilding()
        {
            Guid cityId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
            }

            var createClientBody = new
            {
                type = 1,
                fullName = "John Api Update 400",
                personalIdentificationNumber = "1234567890123",
                email = "john.api.update400@test.com",
                phone = "0700000000",
                address = new { street = "Main St", number = "10" }
            };

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", createClientBody);
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);
            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);

            var createdClient = createdClientResult.Value;
            Assert.NotNull(createdClient);

            var createBuildingBody = new
            {
                cityId = cityId,
                address = new { street = "Dup St", number = "5" },
                constructionYear = 2000,
                type = 1,
                numberOfFloors = 2,
                surfaceArea = 120,
                insuredValue = 150000,
                riskIndicators = 0
            };

            var createBuildingResp =
                await _client.PostAsJsonAsync($"/api/brokers/clients/{createdClient!.Id}/buildings", createBuildingBody);
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuildingResult =
                await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);
            Assert.NotNull(createdBuildingResult);
            Assert.True(createdBuildingResult!.IsSuccess);

            var createdBuilding = createdBuildingResult.Value;
            Assert.NotNull(createdBuilding);

            var invalidUpdateBody = new
            {
                cityId = cityId,
                address = new { street = "Updated St", number = "22" },
                constructionYear = 1700,
                type = 2,
                numberOfFloors = 3,
                surfaceArea = 200,
                insuredValue = 250000,
                riskIndicators = 0
            };

            var updateResp = await _client.PutAsJsonAsync($"/api/brokers/buildings/{createdBuilding!.Id}", invalidUpdateBody);
            Assert.Equal(HttpStatusCode.BadRequest, updateResp.StatusCode);

            var getResp = await _client.GetAsync($"/api/brokers/buildings/{createdBuilding.Id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            var detailsResult =
                await getResp.Content.ReadFromJsonAsync<ApiResult<BuildingDetailsResponse>>(JsonOptions);
            Assert.NotNull(detailsResult);
            Assert.True(detailsResult!.IsSuccess);

            var details = detailsResult.Value;
            Assert.NotNull(details);

            Assert.Equal("DUP ST", details!.Street);
            Assert.Equal("5", details.Number);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private sealed class CreatedClientResponse
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

        private sealed class BuildingDetailsResponse
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public string Street { get; set; } = "";
            public string Number { get; set; } = "";
            public string City { get; set; } = "";
            public string County { get; set; } = "";
            public string Country { get; set; } = "";
        }

        private sealed class ListBuildingsResponse
        {
            public List<BuildingItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class BuildingItem
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