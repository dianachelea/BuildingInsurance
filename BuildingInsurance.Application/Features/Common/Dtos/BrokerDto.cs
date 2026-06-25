using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Application.Features.Common.Dtos
{
    public sealed class BrokerDto
    {
        public Guid Id { get; set; }
        public string BrokerCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public BrokerStatus BrokerStatus { get; set; }
        public decimal? CommissionPercentage { get; set; }
    }
}