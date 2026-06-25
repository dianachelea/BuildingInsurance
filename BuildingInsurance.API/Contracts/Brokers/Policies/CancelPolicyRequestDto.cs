namespace BuildingInsurance.API.Contracts.Brokers.Policies
{
    public sealed class CancelPolicyRequestDto
    {
        public string Reason { get; set; } = null!;
        public DateTime CancellationEffectiveDate { get; set; }
    }
}