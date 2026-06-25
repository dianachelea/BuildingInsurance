using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.GetFeeConfigurationById
{
    public sealed record GetFeeConfigurationByIdQuery(Guid FeeConfigurationId) : IRequest<Result<FeeConfigurationDto>>;
}