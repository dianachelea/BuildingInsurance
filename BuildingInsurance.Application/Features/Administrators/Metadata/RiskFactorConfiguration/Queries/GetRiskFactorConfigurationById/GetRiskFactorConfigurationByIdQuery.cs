using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.GetRiskFactorConfigurationById
{
    public sealed record GetRiskFactorConfigurationByIdQuery(Guid RiskFactorConfigurationId) : IRequest<Result<RiskFactorConfigurationDto>>;
}