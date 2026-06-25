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
    public sealed class ClientBuildingPolicyFullFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ClientBuildingPolicyFullFlowIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;

            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();

            _client = factory.CreateClient();
        }

        [Fact]
        public async Task FullFlow_CreateClient_CreateBuilding_CreateDraftPolicy_Activate_ShouldWork()
        {
            Guid cityId, currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Flow Client",
                personalIdentificationNumber = "9000000000001",
                email = "flow.client@test.com",
                phone = "0710000000",
                address = new { street = "Flow St", number = "1" }
            });

            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var createdClientResult =
                await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions);

            Assert.NotNull(createdClientResult);
            Assert.True(createdClientResult!.IsSuccess);
            Assert.NotEqual(Guid.Empty, createdClientResult.Value!.Id);

            var clientId = createdClientResult.Value.Id;

            var createBuildingResp = await _client.PostAsJsonAsync(
                $"/api/brokers/clients/{clientId}/buildings",
                new
                {
                    cityId = cityId,
                    address = new { street = "Client Flow Ave", number = "10" },
                    constructionYear = 2010,
                    type = 1,
                    numberOfFloors = 2,
                    surfaceArea = 150,
                    insuredValue = 200000,
                    riskIndicators = 0
                });

            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var createdBuildingResult =
                await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions);

            Assert.NotNull(createdBuildingResult);
            Assert.True(createdBuildingResult!.IsSuccess);
            Assert.NotEqual(Guid.Empty, createdBuildingResult.Value!.Id);

            var buildingId = createdBuildingResult.Value.Id;

            var startDate = DateTime.UtcNow.Date.AddDays(1);
            var endDate = startDate.AddYears(1);

            var createPolicyResp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = clientId,
                buildingId = buildingId,
                currencyId = currencyId,
                brokerId = brokerId,
                basePremium = 1000m,
                startDate = startDate,
                endDate = endDate
            });

            Assert.Equal(HttpStatusCode.Created, createPolicyResp.StatusCode);

            var createdPolicyResult =
                await createPolicyResp.Content.ReadFromJsonAsync<ApiResult<CreatedPolicyResponse>>(JsonOptions);

            Assert.NotNull(createdPolicyResult);
            Assert.True(createdPolicyResult!.IsSuccess);
            Assert.NotEqual(Guid.Empty, createdPolicyResult.Value!.Id);

            var policyId = createdPolicyResult.Value.Id;

            var activateResp = await _client.PostAsync($"/api/brokers/policies/{policyId}/activate", null);
            Assert.Equal(HttpStatusCode.OK, activateResp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var policy = await db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == policyId);

                Assert.NotNull(policy);
                Assert.Equal(clientId, policy!.ClientId);
                Assert.Equal(buildingId, policy.BuildingId);
                Assert.Equal(brokerId, policy.BrokerId);
                Assert.Equal(currencyId, policy.CurrencyId);
                Assert.Equal(1000m, policy.BasePremium);
            }
        }

        [Fact]
        public async Task CreateDraftPolicy_InvalidDates_ShouldReturnBadRequest_AndNotPersist()
        {
            Guid cityId, currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Invalid Dates Client",
                personalIdentificationNumber = "9100000000001",
                email = "invalid.dates@test.com",
                phone = "0711111111",
                address = new { street = "A", number = "1" }
            });

            var clientId = (await createClientResp.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!.Id;

            var createBuildingResp = await _client.PostAsJsonAsync(
                $"/api/brokers/clients/{clientId}/buildings",
                new
                {
                    cityId = cityId,
                    address = new { street = "B", number = "2" },
                    constructionYear = 2000,
                    type = 1,
                    numberOfFloors = 2,
                    surfaceArea = 120,
                    insuredValue = 150000,
                    riskIndicators = 0
                });

            var buildingId = (await createBuildingResp.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!.Id;

            var resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = clientId,
                buildingId = buildingId,
                currencyId = currencyId,
                brokerId = brokerId,
                basePremium = 500m,
                startDate = new DateTime(2027, 1, 1),
                endDate = new DateTime(2026, 1, 1)
            });

            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                var count = await db.Policies.CountAsync(p => p.BasePremium == 500m);
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task Activate_ThenCancel_ThenActivateAgain_ShouldFail()
        {
            Guid cityId, currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var clientId = (await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Cancel Reactivate Client",
                personalIdentificationNumber = "9300000000001",
                email = "cancel.reactivate@test.com",
                phone = "0713333333",
                address = new { street = "ACA", number = "1" }
            }).Result.Content.ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!.Id;

            var buildingId = (await _client.PostAsJsonAsync(
                $"/api/brokers/clients/{clientId}/buildings",
                new
                {
                    cityId = cityId,
                    address = new { street = "ACA St", number = "2" },
                    constructionYear = 2015,
                    type = 1,
                    numberOfFloors = 1,
                    surfaceArea = 90,
                    insuredValue = 120000,
                    riskIndicators = 0
                }).Result.Content.ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!.Id;

            var policyId = (await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = clientId,
                buildingId = buildingId,
                currencyId = currencyId,
                brokerId = brokerId,
                basePremium = 800m,
                startDate = DateTime.UtcNow.Date.AddDays(1),
                endDate = DateTime.UtcNow.Date.AddYears(1)
            }).Result.Content.ReadFromJsonAsync<ApiResult<CreatedPolicyResponse>>(JsonOptions))!.Value!.Id;

            await _client.PostAsync($"/api/brokers/policies/{policyId}/activate", null);

            var cancelResp = await _client.PostAsJsonAsync($"/api/brokers/policies/{policyId}/cancel", new
            {
                reason = "Customer request",
                cancellationEffectiveDate = DateTime.UtcNow.Date.AddDays(10)
            });

            Assert.Equal(HttpStatusCode.OK, cancelResp.StatusCode);

            var activateAgain = await _client.PostAsync($"/api/brokers/policies/{policyId}/activate", null);

            Assert.True(activateAgain.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeactivateBroker_ThenCreateNewPolicy_ShouldFail_ButExistingPoliciesRemainUnaffected()
        {
            Guid cityId, currencyId, brokerId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
                cityId = await db.Cities.Select(c => c.Id).FirstAsync();
                currencyId = await db.Currencies.Select(c => c.Id).FirstAsync();
                brokerId = await db.Brokers.Select(b => b.Id).FirstAsync();
            }

            var createClientResp = await _client.PostAsJsonAsync("/api/brokers/clients", new
            {
                type = 1,
                fullName = "Broker Deactivation Client",
                personalIdentificationNumber = "9400000000001",
                email = "broker.deactivation@test.com",
                phone = "0714444444",
                address = new { street = "BD", number = "1" }
            });
            Assert.Equal(HttpStatusCode.Created, createClientResp.StatusCode);

            var clientId = (await createClientResp.Content
                .ReadFromJsonAsync<ApiResult<CreatedClientResponse>>(JsonOptions))!.Value!.Id;

            var createBuildingResp = await _client.PostAsJsonAsync(
                $"/api/brokers/clients/{clientId}/buildings",
                new
                {
                    cityId = cityId,
                    address = new { street = "BD St", number = "2" },
                    constructionYear = 2012,
                    type = 1,
                    numberOfFloors = 1,
                    surfaceArea = 100,
                    insuredValue = 130000,
                    riskIndicators = 0
                });
            Assert.Equal(HttpStatusCode.Created, createBuildingResp.StatusCode);

            var buildingId = (await createBuildingResp.Content
                .ReadFromJsonAsync<ApiResult<CreatedBuildingResponse>>(JsonOptions))!.Value!.Id;

            var start1 = DateTime.UtcNow.Date.AddDays(1);
            var end1 = start1.AddYears(1);

            var createPolicy1Resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = clientId,
                buildingId = buildingId,
                currencyId = currencyId,
                brokerId = brokerId,
                basePremium = 750m,
                startDate = start1,
                endDate = end1
            });
            Assert.Equal(HttpStatusCode.Created, createPolicy1Resp.StatusCode);

            var policy1Id = (await createPolicy1Resp.Content
                .ReadFromJsonAsync<ApiResult<CreatedPolicyResponse>>(JsonOptions))!.Value!.Id;

            Assert.NotEqual(Guid.Empty, policy1Id);

            var deactivateBrokerResp = await _client.PostAsync($"/api/admin/brokers/{brokerId}/deactivate", null);
            Assert.Equal(HttpStatusCode.OK, deactivateBrokerResp.StatusCode);

            var start2 = DateTime.UtcNow.Date.AddDays(2);
            var end2 = start2.AddYears(1);

            var createPolicy2Resp = await _client.PostAsJsonAsync("/api/brokers/policies", new
            {
                clientId = clientId,
                buildingId = buildingId,
                currencyId = currencyId,
                brokerId = brokerId,
                basePremium = 760m,
                startDate = start2,
                endDate = end2
            });

            Assert.True(createPolicy2Resp.StatusCode == HttpStatusCode.BadRequest);

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var policy1 = await db.Policies.AsNoTracking().FirstOrDefaultAsync(p => p.Id == policy1Id);
                Assert.NotNull(policy1);
                Assert.Equal(brokerId, policy1!.BrokerId);

                var broker = await db.Brokers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == brokerId);
                Assert.NotNull(broker);
                Assert.Equal(Domain.Enums.BrokerStatus.Inactive, broker!.BrokerStatus);

                var policy2Count = await db.Policies.AsNoTracking().CountAsync(p => p.BasePremium == 760m && p.ClientId == clientId);
                Assert.Equal(0, policy2Count);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private sealed class CreatedClientResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class CreatedBuildingResponse
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public Guid CityId { get; set; }
        }

        private sealed class CreatedPolicyResponse
        {
            public Guid Id { get; set; }
        }

        private sealed class PolicyDetailsResponse
        {
            public Guid Id { get; set; }
            public Guid ClientId { get; set; }
            public Guid BuildingId { get; set; }
            public decimal BasePremium { get; set; }
            public string Status { get; set; } = "";
        }

        private sealed class ApiResult<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
        }
    }
}