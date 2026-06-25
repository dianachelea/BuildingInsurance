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
    public sealed class GeographyEndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public GeographyEndpointsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            var cs = TestConfig.GetTestConnectionString();
            TestDbSeeder.ResetAndSeedAsync(cs).GetAwaiter().GetResult();
            _client = factory.CreateClient();
        }

        private async Task<Guid> GetAnyCountryIdAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
            return await db.Countries.Select(c => c.Id).FirstAsync();
        }

        private async Task<Guid> GetAnyCountyIdAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();
            return await db.Counties.Select(c => c.Id).FirstAsync();
        }

        [Fact]
        public async Task GetCountries_ShouldReturnSeededCountry()
        {
            var resp = await _client.GetAsync("/api/brokers/countries?page=1&pageSize=50");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            
            var result = await resp.Content.ReadFromJsonAsync<ResultResponse<CountryListResponse>>(
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var list = result.Value;
            Assert.NotNull(list);

            Assert.True(list!.TotalCount >= 1);
            Assert.NotEmpty(list.Items);
            Assert.Contains(list.Items, c => c.Name == "ROMANIA");
        }

        [Fact]
        public async Task GetCountries_Paging_ShouldReturnDifferentPages()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                if (await db.Countries.CountAsync() < 3)
                {
                    var c2 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "TestCountry1");
                    var c3 = new Domain.Entities.Geography.Country(Guid.NewGuid(), "TestCountry2");
                    db.Countries.AddRange(c2, c3);
                    await db.SaveChangesAsync();
                }
            }

            var page1Resp = await _client.GetAsync("/api/brokers/countries?page=1&pageSize=1");
            Assert.Equal(HttpStatusCode.OK, page1Resp.StatusCode);
            var page1 = await page1Resp.Content.ReadFromJsonAsync<CountryListResponse>(JsonOptions);
            Assert.NotNull(page1);
            Assert.True(page1!.Items.Count <= 1);

            var page2Resp = await _client.GetAsync("/api/brokers/countries?page=2&pageSize=1");
            Assert.Equal(HttpStatusCode.OK, page2Resp.StatusCode);
            var page2 = await page2Resp.Content.ReadFromJsonAsync<CountryListResponse>(JsonOptions);
            Assert.NotNull(page2);
            Assert.True(page2!.Items.Count <= 1);

            if (page1.Items.Count > 0 && page2.Items.Count > 0)
            {
                Assert.DoesNotContain(page2.Items[0].Id, page1.Items.Select(x => x.Id));
            }
        }

        [Fact]
        public async Task GetCountiesByCountry_ShouldReturnCountiesForCountry()
        {
            var countryId = await GetAnyCountryIdAsync();

            var resp = await _client.GetAsync($"/api/brokers/countries/{countryId}/counties?page=1&pageSize=100");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
            
            var result = await resp.Content.ReadFromJsonAsync<ResultResponse<CountryListResponse>>(JsonOptions);
            
            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var list = result.Value;

            Assert.True(list!.TotalCount >= 1);
            Assert.NotEmpty(list.Items);
            Assert.Contains(list.Items, c => c.Name == "CLUJ");
        }

        [Fact]
        public async Task GetCountiesByCountry_WhenCountryHasNoCounties_ShouldReturnEmptyList()
        {
            Guid emptyCountryId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var emptyCountry = new Domain.Entities.Geography.Country(Guid.NewGuid(), "EmptyCountry");
                db.Countries.Add(emptyCountry);
                await db.SaveChangesAsync();

                emptyCountryId = emptyCountry.Id;
            }

            var resp = await _client.GetAsync($"/api/brokers/countries/{emptyCountryId}/counties?page=1&pageSize=100");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var list = await resp.Content.ReadFromJsonAsync<CountyListResponse>(JsonOptions);
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        [Fact]
        public async Task GetCountiesByCountry_WhenCountryDoesNotExist_ShouldReturnEmptyList()
        {
            var resp = await _client.GetAsync($"/api/brokers/countries/{Guid.NewGuid()}/counties?page=1&pageSize=100");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var list = await resp.Content.ReadFromJsonAsync<CountyListResponse>(JsonOptions);
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        [Fact]
        public async Task GetCitiesByCounty_ShouldReturnCitiesForCounty()
        {
            var countyId = await GetAnyCountyIdAsync();

            var resp = await _client.GetAsync($"/api/brokers/counties/{countyId}/cities?page=1&pageSize=50");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var result = await resp.Content.ReadFromJsonAsync<ResultResponse<CountryListResponse>>(JsonOptions);

            Assert.NotNull(result);
            Assert.True(result!.IsSuccess);

            var list = result.Value;

            Assert.True(list!.TotalCount >= 1);
            Assert.NotEmpty(list.Items);
            Assert.Contains(list.Items, c => c.Name == "CLUJ-NAPOCA");
        }

        [Fact]
        public async Task GetCitiesByCounty_WhenCountyHasNoCities_ShouldReturnEmptyList()
        {
            Guid emptyCountyId;
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BuildingInsuranceDbContext>();

                var countryId = await db.Countries.Select(c => c.Id).FirstAsync();
                var emptyCounty = new Domain.Entities.Geography.County(Guid.NewGuid(), "EmptyCounty", countryId);
                db.Counties.Add(emptyCounty);
                await db.SaveChangesAsync();

                emptyCountyId = emptyCounty.Id;
            }

            var resp = await _client.GetAsync($"/api/brokers/counties/{emptyCountyId}/cities?page=1&pageSize=50");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var list = await resp.Content.ReadFromJsonAsync<CityListResponse>(JsonOptions);
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        [Fact]
        public async Task GetCitiesByCounty_WhenCountyDoesNotExist_ShouldReturnEmptyList()
        {
            var resp = await _client.GetAsync($"/api/brokers/counties/{Guid.NewGuid()}/cities?page=1&pageSize=50");
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

            var list = await resp.Content.ReadFromJsonAsync<CityListResponse>(JsonOptions);
            Assert.NotNull(list);

            Assert.Equal(0, list!.TotalCount);
            Assert.Empty(list.Items);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private sealed class CountryListResponse
        {
            public List<CountryItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class CountryItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
        }

        private sealed class CountyListResponse
        {
            public List<CountyItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class CountyItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
        }

        private sealed class CityListResponse
        {
            public List<CityItem> Items { get; set; } = new();
            public int TotalPages { get; set; }
            public int TotalCount { get; set; }
        }

        private sealed class CityItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
        }

        private sealed class ResultResponse<T>
        {
            public T? Value { get; set; }
            public bool IsSuccess { get; set; }
            public bool IsFailure { get; set; }
            public string Error { get; set; } = "";
            public string ErrorType { get; set; } = "";
        }
    }
}