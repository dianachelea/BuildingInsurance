using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker
{
    public sealed record ActivateBrokerCommand(Guid BrokerId) : ICommand<Result<BrokerDto>>;
}