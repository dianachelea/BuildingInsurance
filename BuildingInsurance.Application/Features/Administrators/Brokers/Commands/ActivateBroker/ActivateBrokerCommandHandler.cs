using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker
{
    public sealed class ActivateBrokerCommandHandler : IRequestHandler<ActivateBrokerCommand, Result<BrokerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActivateBrokerCommandHandler> _logger;

        public ActivateBrokerCommandHandler(IUnitOfWork unitOfWork, ILogger<ActivateBrokerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BrokerDto>> Handle(ActivateBrokerCommand request, CancellationToken cancellationToken)
        {
            var broker = await _unitOfWork.Brokers.GetByIdAsync(request.BrokerId, cancellationToken);
            if (broker is null)
                return Result<BrokerDto>.Failure($"Broker with ID {request.BrokerId} not found.", ErrorType.NotFound);

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                if (broker.BrokerStatus != BrokerStatus.Active)
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    transactionStarted = true;

                    broker.Activate();

                    await _unitOfWork.CommitAsync(cancellationToken);
                    committed = true;

                    _logger.LogInformation("Broker activated. BrokerId={BrokerId}", broker.Id);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during broker activation. BrokerId={BrokerId}", request.BrokerId);
                return Result<BrokerDto>.Failure("Unexpected error during broker activation.",ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                    }
                    catch (Exception rbEx)
                    {
                        _logger.LogWarning(rbEx,"Final rollback attempt failed during broker activation. BrokerId={BrokerId}", request.BrokerId);
                    }
                }
            }
        }
    }
}