using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Metadata;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers
{
    public sealed class CurrencyVerifier : ICurrencyVerifier
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CurrencyVerifier> _logger;

        public CurrencyVerifier(IUnitOfWork unitOfWork, ILogger<CurrencyVerifier> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Currency>> GetActiveAsync(Guid currencyId, CancellationToken cancellationToken)
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(currencyId, cancellationToken);
            if (currency is null)
            {
                _logger.LogDebug("Currency with ID {CurrencyId} not found.", currencyId);
                return Result<Currency>.Failure("Currency not found.", ErrorType.NotFound);
            }

            if (!currency.IsActive)
            {
                _logger.LogDebug("Currency with ID {CurrencyId} is inactive.", currencyId);
                return Result<Currency>.Failure("Currency is inactive.", ErrorType.Validation);
            }

            return Result<Currency>.Success(currency);
        }
    }
}