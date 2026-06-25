using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities
{
    public class ListCitiesByCountyHandler : IRequestHandler<ListCitiesByCountyQuery, Result<ListCitiesByCountyResponse>>
    {
        private readonly ICityRepository _cityRepository;

        public ListCitiesByCountyHandler(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<Result<ListCitiesByCountyResponse>> Handle(ListCitiesByCountyQuery request, CancellationToken cancellationToken)
        {
            var (citiesFromDb, totalCount) = await _cityRepository.GetByCountyIdPagedAsync(request.CountyId, request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = citiesFromDb
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CountyId = c.CountyId
                })
                .ToList();

            var response = new ListCitiesByCountyResponse(
                Items: items,
                TotalPages: totalPages,
                TotalCount: totalCount);

            return Result<ListCitiesByCountyResponse>.Success(response);
        }
    }
}