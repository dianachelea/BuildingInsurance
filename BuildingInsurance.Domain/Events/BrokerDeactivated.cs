namespace BuildingInsurance.Domain.Events
{
    public sealed class BrokerDeactivated : IDomainEvent
    {
        public Guid BrokerId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public BrokerDeactivated(Guid brokerId)
        {
            BrokerId = brokerId;
        }
    }
}
