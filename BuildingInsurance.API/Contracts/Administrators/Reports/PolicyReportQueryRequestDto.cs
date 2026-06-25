using BuildingInsurance.API.Contracts.Brokers.Buildings;
using BuildingInsurance.API.Contracts.Brokers.Policies;

namespace BuildingInsurance.API.Contracts.Administrators.Reports
{
    public sealed class PolicyReportQueryRequestDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public PolicyStatusRequestDto? Status { get; set; }
        public string? Currency { get; set; }
        public BuildingTypeRequestDto? BuildingType { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}