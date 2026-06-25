using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class PolicyDetailsDto
    {
        public Guid Id { get; init; }
        public string PolicyNumber { get; init; } = null!;
        public PolicyStatus PolicyStatus { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal BasePremium { get; init; }
        public decimal FinalPremium { get; init; }
        public decimal FinalPremiumInBaseCurrency { get; set; }
        public decimal? EstimatedFinalPremium { get; set; }
        public string Currency { get; init; } = null!;
        public DateTime? CancellationEffectiveDate { get; init; }
        public ClientDetailsDto Client { get; init; } = null!;
        public BuildingDetailsDto Building { get; init; } = null!;
        public IReadOnlyList<PolicyAppliedFeeDto> AppliedFees { get; init; } = [];
        public IReadOnlyList<PolicyAppliedRiskFactorDto> AppliedRiskFactors { get; init; } = [];
    }
}