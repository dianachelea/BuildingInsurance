namespace BuildingInsurance.Domain.Events
{
    public sealed class ClientIdentifierChanged : IDomainEvent
    {
        public Guid ClientId { get; }
        public string OldValue { get; }
        public string NewValue { get; }
        public string Reason { get; }
        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ClientIdentifierChanged(Guid clientId, string oldValue, string newValue, string reason)
        {
            ClientId = clientId;
            OldValue = oldValue;
            NewValue = newValue;
            Reason = reason;
        }
    }
}