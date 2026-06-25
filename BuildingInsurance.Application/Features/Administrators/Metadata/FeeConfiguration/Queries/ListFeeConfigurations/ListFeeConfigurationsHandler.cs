using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations
{
    public sealed class ListFeeConfigurationsHandler : IRequestHandler<ListFeeConfigurationsQuery, Result<ListFeeConfigurationsResponse>>
    {
        private readonly IFeeConfigurationRepository _feeConfigurationRepository;

        public ListFeeConfigurationsHandler(IFeeConfigurationRepository feeConfigurationRepository)
        {
            _feeConfigurationRepository = feeConfigurationRepository;
        }

        public async Task<Result<ListFeeConfigurationsResponse>> Handle(ListFeeConfigurationsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _feeConfigurationRepository.SearchPagedAsync(request.Name, request.Type.MapToDomainFeeTypeOptional(), request.IsActive, request.Page, request.PageSize, cancellationToken);
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var dtos = items.Select(f => new FeeConfigurationDto
            {
                Id = f.Id,
                Name = f.Name,
                FeeType = f.FeeType,
                FeePercentage = f.FeePercentage,
                EffectiveFrom = f.EffectiveFrom,
                EffectiveTo = f.EffectiveTo,
                IsActive = f.IsActive,
                RiskIndicators = f.RiskIndicators
            }).ToList();

            var response = new ListFeeConfigurationsResponse(dtos, totalPages, totalCount);
            return Result<ListFeeConfigurationsResponse>.Success(response);
        }
    }
}