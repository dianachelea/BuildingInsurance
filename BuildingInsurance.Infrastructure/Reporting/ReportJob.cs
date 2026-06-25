namespace BuildingInsurance.Infrastructure.Reporting
{
    public sealed class ReportJob
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = "Queued";
        public int Progress { get; set; } = 0;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public string PayloadJson { get; set; } = string.Empty;
        public string? Error { get; set; }
        public Guid? ResultId { get; set; }
    }
}