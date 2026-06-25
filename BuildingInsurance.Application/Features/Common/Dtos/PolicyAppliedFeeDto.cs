namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class PolicyAppliedFeeDto
    {
        public Guid FeeConfigurationId { get; init; }
        public string FeeName { get; init; } = null!;
        public decimal Percentage { get; init; }
        public DateTime AppliedAtUtc { get; init; }
    }
}