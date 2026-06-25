namespace BuildingInsurance.Domain.Events
{
    public sealed class BrokerCreated : IDomainEvent
    {
        public Guid BrokerId { get; }
        public string BrokerCode { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public BrokerCreated(Guid brokerId, string brokerCode)
        {
            BrokerId = brokerId;
            BrokerCode = brokerCode;
        }
    }
}