using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCurrencyCommandHandler> _logger;

        public CreateCurrencyCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateCurrencyCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Currencies.GetByCodeAsync(request.Code, cancellationToken);
            if (existing is not null)
            {
                _logger.LogWarning("Currency code already exists: {Code}", request.Code);
                return Result<CurrencyDto>.Conflict("Currency code already exists.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var currency = new Domain.Entities.Metadata.Currency(
                    code: request.Code,
                    name: request.Name,
                    exchangeRateToBase: request.ExchangeRateToBase,
                    isActive: request.IsActive);

                await _unitOfWork.Currencies.AddAsync(currency, cancellationToken);

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
                _logger.LogError(ex, "Unexpected error during currency creation for Code = {Code}", request.Code);
                return Result<CurrencyDto>.Failure("Unexpected error during currency creation.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Final rollback attempt failed during currency creation for Code = {Code}", request.Code);
                    }
                }
            }
        }
    }
}