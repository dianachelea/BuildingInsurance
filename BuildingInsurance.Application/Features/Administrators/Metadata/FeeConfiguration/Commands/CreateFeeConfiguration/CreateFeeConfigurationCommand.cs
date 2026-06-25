using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration
{
    public sealed class CreateFeeConfigurationCommand : ICommand<Result<FeeConfigurationDto>>
    {
        public string Name { get; set; } = null!;
        public FeeTypeContract FeeType { get; set; }
        public decimal FeePercentage { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public RiskIndicatorsContract RiskIndicators { get; set; }
    }
}