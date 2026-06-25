namespace BuildingInsurance.Infrastructure.Reporting
{
    public sealed class ReportJobResult
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string RowsJson { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}