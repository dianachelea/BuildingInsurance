using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker
{
    public sealed class CreateBrokerCommandHandler : IRequestHandler<CreateBrokerCommand, Result<BrokerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateBrokerCommandHandler> _logger;

        public CreateBrokerCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateBrokerCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<BrokerDto>> Handle(CreateBrokerCommand request, CancellationToken cancellationToken)
        {
            if (await _unitOfWork.Brokers.BrokerCodeExistsAsync(request.BrokerCode!, cancellationToken))
            {
                _logger.LogWarning("Broker creation conflict. BrokerCode already exists: {BrokerCode}", request.BrokerCode);

                return Result<BrokerDto>.Conflict("Another broker already exists with the same broker code.");
            }

            if (await _unitOfWork.Brokers.BrokerEmailExistsAsync(request.Email!, cancellationToken))
            {
                _logger.LogWarning("Broker creation conflict. Email already exists: {Email}", request.Email);

                return Result<BrokerDto>.Conflict("Another broker already exists with the same email address.");
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                var broker = new Broker(
                    brokerCode: request.BrokerCode,
                    name: request.FullName,
                    contactInfo: new ContactInfo(request.Email, request.Phone),
                    brokerStatus: BrokerStatus.Inactive,
                    commissionPercentage: request.CommissionPercentage
                );

                broker.MarkAsCreated();

                await _unitOfWork.Brokers.AddAsync(broker, cancellationToken);

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
                _logger.LogError(ex, "Unexpected error during broker creation for BrokerCode={BrokerCode}", request.BrokerCode);
                return Result<BrokerDto>.Failure("Unexpected error during broker creation.", ErrorType.Generic);
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
                        _logger.LogWarning(rbEx, "Final rollback attempt failed during broker creation for BrokerCode={BrokerCode}", request.BrokerCode);
                    }
                }
            }
        }
    }
}