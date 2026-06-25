using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common
{
    public interface IRiskFactorTargetRequest
    {
        RiskFactorLevelContract Level { get; }
        Guid? ReferenceId { get; }
        BuildingTypeContract? BuildingType { get; }
    }
}