namespace BuildingInsurance.Domain.Events
{
    public sealed class BrokerActivated : IDomainEvent
    {
        public Guid BrokerId { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public BrokerActivated(Guid brokerId)
        {
            BrokerId = brokerId;
        }
    }
}
