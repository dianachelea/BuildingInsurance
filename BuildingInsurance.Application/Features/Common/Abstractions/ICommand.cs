using MediatR;

namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }
}