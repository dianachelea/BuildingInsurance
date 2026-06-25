using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration
{
    public sealed class CreateRiskFactorConfigurationCommand : ICommand<Result<RiskFactorConfigurationDto>>, IRiskFactorTargetRequest
    {
        public RiskFactorLevelContract Level { get; set; }
        public Guid? ReferenceId { get; set; }
        public BuildingTypeContract? BuildingType { get; set; } = null;
        public decimal AdjustmentPercentage { get; set; }
        public bool IsActive { get; set; }
    }
}