using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker
{
    public sealed class UpdateBrokerCommandHandler : IRequestHandler<UpdateBrokerCommand, Result<BrokerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateBrokerCommandHandler> _logger;

        public UpdateBrokerCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateBrokerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BrokerDto>> Handle(UpdateBrokerCommand request, CancellationToken cancellationToken)
        {
            var broker = await _unitOfWork.Brokers.GetByIdAsync(request.Id, cancellationToken);
            if (broker is null)
            {
                _logger.LogWarning("Broker with ID {BrokerId} not found.", request.Id);
                return Result<BrokerDto>.Failure($"Broker with ID {request.Id} not found.", ErrorType.NotFound);
            }

            var newEmailNormalized = request.Email.Trim().ToLowerInvariant();
            if (broker.ContactInfo.Email != newEmailNormalized)
            {
                var emailExists = await _unitOfWork.Brokers.BrokerEmailExistsAsync(request.Email, cancellationToken);
                if (emailExists)
                {
                    _logger.LogWarning("Attempt to update broker {BrokerId} with duplicate email {Email}.", broker.Id, request.Email);
                    return Result<BrokerDto>.Conflict("Another broker already exists with the same email address.");
                }
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                broker.UpdateName(request.FullName);
                broker.UpdateCommission(request.CommissionPercentage);
                broker.UpdateContact(new ContactInfo(request.Email, request.Phone));

                _unitOfWork.Brokers.Update(broker);

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

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
                _logger.LogError(ex, "Unexpected error while updating broker {BrokerId}", request.Id);
                return Result<BrokerDto>.Failure("Unexpected error while updating broker.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Rollback failed while updating broker {BrokerId}", request.Id);
                    }
                }
            }
        }
    }
}