using BuildingInsurance.Domain.Events;

namespace BuildingInsurance.Domain.Entities.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }

        private OutboxMessage() { }

        public OutboxMessage(IDomainEvent domainEvent)
        {
            Type = domainEvent.GetType().FullName!;
            Payload = System.Text.Json.JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
            OccurredOn = domainEvent.OccurredOn;
        }

        public void MarkProcessed() => ProcessedOn = DateTime.UtcNow;

        public void MarkFailed(string error) => Error = error;
    }
}