using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties
{
    public sealed class ListCountiesByCountryHandler : IRequestHandler<ListCountiesByCountryQuery, Result<ListCountiesByCountryResponse>>
    {
        private readonly ICountyRepository _countyRepository;

        public ListCountiesByCountryHandler(ICountyRepository countyRepository)
        {
            _countyRepository = countyRepository;
        }

        public async Task<Result<ListCountiesByCountryResponse>> Handle(ListCountiesByCountryQuery request, CancellationToken cancellationToken)
        {
            var (countiesFromDb, totalCount) = await _countyRepository.GetByCountryIdPagedAsync(request.CountryId, request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var items = countiesFromDb
                    .Select(c => new CountyDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CountryId = c.CountryId
                    })
                    .ToList();

            var response = new ListCountiesByCountryResponse(items, totalPages, totalCount);
            return Result<ListCountiesByCountryResponse>.Success(response);
        }
    }
}