using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers
{
    public sealed class BrokerVerifier : IBrokerVerifier
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BrokerVerifier> _logger;

        public BrokerVerifier(IUnitOfWork unitOfWork, ILogger<BrokerVerifier> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Broker>> GetActiveAsync(Guid brokerId, CancellationToken cancellationToken)
        {
            var broker = await _unitOfWork.Brokers.GetByIdAsync(brokerId, cancellationToken);
            if (broker is null)
            {
                _logger.LogDebug("Broker with ID {BrokerId} not found.", brokerId);
                return Result<Broker>.Failure("Broker not found.", ErrorType.NotFound);
            }

            if (broker.BrokerStatus == BrokerStatus.Inactive)
            {
                _logger.LogDebug("Broker with ID {BrokerId} is inactive.", brokerId);
                return Result<Broker>.Failure("Broker is inactive.", ErrorType.Validation);
            }

            return Result<Broker>.Success(broker);
        }
    }
}