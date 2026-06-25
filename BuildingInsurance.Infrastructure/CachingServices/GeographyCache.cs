using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingInsurance.Infrastructure.HostedServices
{
    public sealed class GeographyCache : IGeographyCachingService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private Dictionary<Guid, (string City, string County, string Country)> _data = new();

        public GeographyCache(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LoadAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var countries = await unitOfWork.Countries.GetAllAsync(ct);
            var counties = await unitOfWork.Counties.GetAllAsync(ct);
            var cities = await unitOfWork.Cities.GetAllAsync(ct);

            var countryById = countries.ToDictionary(x => x.Id, x => x.Name);
            var countyById = counties.ToDictionary(x => x.Id, x => (x.Name, x.CountryId));

            var map = new Dictionary<Guid, (string, string, string)>();

            foreach (var city in cities)
            {
                if (!countyById.TryGetValue(city.CountyId, out var county))
                    continue;

                if (!countryById.TryGetValue(county.CountryId, out var country))
                    continue;

                map[city.Id] = (city.Name, county.Name, country);
            }

            _data = map;
        }

        public bool TryGet(Guid cityId, out string city, out string county, out string country)
        {
            if (_data.TryGetValue(cityId, out var geo))
            {
                city = geo.City;
                county = geo.County;
                country = geo.Country;
                return true;
            }

            city = string.Empty;
            county = string.Empty;
            country = string.Empty;
            return false;
        }
    }
}