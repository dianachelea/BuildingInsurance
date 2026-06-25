using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency
{
    public sealed class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCurrencyCommandHandler> _logger;
        public UpdateCurrencyCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCurrencyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(request.Id, cancellationToken);
            if (currency is null)
            {
                _logger.LogWarning("Currency with ID {CurrencyId} not found.", request.Id);
                return Result<CurrencyDto>.Failure($"Currency with ID {request.Id} not found.", ErrorType.NotFound);
            }

            var tryingToDeactivate = currency.IsActive && !request.IsActive;
            if (tryingToDeactivate)
            {
                var isUsed = await _unitOfWork.Currencies.IsUsedInActivePoliciesAsync(currency.Id, cancellationToken);
                if (isUsed)
                {
                    _logger.LogWarning("Attempt to deactivate currency {CurrencyId} which is used by active policies.", currency.Id);
                    return Result<CurrencyDto>.Conflict("Cannot deactivate currency which is used in active policies.");
                }
            }

            bool transactionStarted = false;
            bool committed = false;
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                currency.UpdateName(request.Name);
                currency.UpdateExchangeRateToBase(request.ExchangeRateToBase);

                if (request.IsActive)
                    currency.Activate();
                else
                    currency.Deactivate();

                _unitOfWork.Currencies.Update(currency);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                var dto = new CurrencyDto
                {
                    Id = currency.Id,
                    Code = currency.Code,
                    Name = currency.Name,
                    ExchangeRateToBase = currency.ExchangeRateToBase,
                    IsActive = currency.IsActive
                };

                return Result<CurrencyDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating currency {CurrencyId}", request.Id);
                return Result<CurrencyDto>.Failure("Unexpected error while updating currency.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed while updating currency {CurrencyId}", request.Id);
                    }
                }
            }
        }
    }
}