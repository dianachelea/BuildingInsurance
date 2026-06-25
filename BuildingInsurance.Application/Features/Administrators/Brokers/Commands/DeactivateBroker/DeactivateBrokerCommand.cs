using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.DeactivateBroker
{
    public sealed record DeactivateBrokerCommand(Guid BrokerId) : ICommand<Result<BrokerDto>>;
}