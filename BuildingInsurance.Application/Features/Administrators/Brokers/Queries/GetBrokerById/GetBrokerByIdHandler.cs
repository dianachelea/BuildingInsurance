using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Queries.GetBrokerById
{
    public sealed class GetBrokerByIdHandler : IRequestHandler<GetBrokerByIdQuery, Result<BrokerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetBrokerByIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<BrokerDto>> Handle(GetBrokerByIdQuery request, CancellationToken cancellationToken)
        {
            var broker = await _unitOfWork.Brokers.GetByIdAsync(request.BrokerId, cancellationToken);
            if (broker is null)
            {
                return Result<BrokerDto>.Failure($"Broker with ID {request.BrokerId} not found.", ErrorType.NotFound);
            }

            var dto = new BrokerDto
            {
                Id = broker.Id,
                BrokerCode = broker.BrokerCode,
                FullName = broker.FullName,
                Email = broker.ContactInfo.Email,
                Phone = broker.ContactInfo.Phone,
                BrokerStatus = broker.BrokerStatus,
                CommissionPercentage = broker.CommissionPercentage
            };

            return Result<BrokerDto>.Success(dto);
        }
    }
}