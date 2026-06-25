namespace BuildingInsurance.Infrastructure.Persistence.Cursors
{
    public sealed class ProcessingCheckpoint
    {
        public string Name { get; set; } = string.Empty;
        public long LastProcessedChangeVersion { get; set; }
    }
}