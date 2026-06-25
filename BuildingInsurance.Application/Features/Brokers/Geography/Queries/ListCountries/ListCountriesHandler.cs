using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using MediatR;

namespace BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries
{
    public sealed class ListCountriesHandler : IRequestHandler<ListCountriesQuery, Result<ListCountriesResponse>>
    {
        private readonly ICountryRepository _countryRepository;

        public ListCountriesHandler(ICountryRepository countryRepository)
        {
            _countryRepository= countryRepository;
        }

        public async Task<Result<ListCountriesResponse>> Handle(ListCountriesQuery request, CancellationToken cancellationToken)
        {
            var (countriesFromDb, totalCount) = await _countryRepository.GetAllCountriesPagedAsync(request.Page, request.PageSize, cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var items = countriesFromDb
                .Select(c => new CountryDto { Id = c.Id, Name = c.Name })
                .ToList();

            return Result<ListCountriesResponse>.Success(new ListCountriesResponse(items, totalPages, totalCount));
        }
    }
}