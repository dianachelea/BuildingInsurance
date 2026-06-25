namespace BuildingInsurance.Domain.Events
{
    public class PolicyCancelled : IDomainEvent
    {
        public Guid PolicyId { get; }
        public string Reason { get; }
        public DateTime EffectiveDate { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public PolicyCancelled(Guid policyId, string reason, DateTime effectiveDate)
        {
            PolicyId = policyId;
            Reason = reason;
            EffectiveDate = effectiveDate;
        }
    }
}