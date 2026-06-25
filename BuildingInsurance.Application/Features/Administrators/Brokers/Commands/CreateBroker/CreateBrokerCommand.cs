using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker
{
    public sealed class CreateBrokerCommand : ICommand<Result<BrokerDto>>
    {
        public string BrokerCode { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public decimal? CommissionPercentage { get; set; }
    }
}