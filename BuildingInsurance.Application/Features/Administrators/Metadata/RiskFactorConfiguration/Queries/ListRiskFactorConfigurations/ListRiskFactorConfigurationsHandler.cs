using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations
{
    public sealed class ListRiskFactorConfigurationsHandler : IRequestHandler<ListRiskFactorConfigurationsQuery, Result<ListRiskFactorConfigurationsResponse>>
    {
        private readonly IRiskFactorConfigurationRepository _riskFactorConfigurationRepository;

        public ListRiskFactorConfigurationsHandler(IRiskFactorConfigurationRepository riskFactorConfigurationRepository)
        {
            _riskFactorConfigurationRepository = riskFactorConfigurationRepository;
        }

        public async Task<Result<ListRiskFactorConfigurationsResponse>> Handle(ListRiskFactorConfigurationsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _riskFactorConfigurationRepository.SearchPagedAsync(
                level: request.Level.MapToDomainRiskFactorLevelOptional(),
                referenceId: request.ReferenceId,
                isActive: request.IsActive,
                page: request.Page,
                pageSize: request.PageSize,
                ct: cancellationToken);

            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var dtos = items.Select(r => new RiskFactorConfigurationDto
            {
                Id = r.Id,
                Level = r.Level,
                ReferenceId = r.ReferenceId,
                BuildingType = r.BuildingType,
                AdjustmentPercentage = r.AdjustmentPercentage,
                IsActive = r.IsActive
            }).ToList();

            var response = new ListRiskFactorConfigurationsResponse(dtos, totalPages, totalCount);
            return Result<ListRiskFactorConfigurationsResponse>.Success(response);
        }
    }
}