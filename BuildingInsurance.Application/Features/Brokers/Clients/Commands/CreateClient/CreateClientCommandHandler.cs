using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using BuildingInsurance.Application.Features.Common.Mapping;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Result<ClientDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateClientCommandHandler> _logger;

        public CreateClientCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateClientCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            if (await _unitOfWork.Clients.EmailExistsAsync(request.Email, cancellationToken))
            {
                _logger.LogWarning("Client creation failed. Email already exists. Email={Email}", request.Email);
                return Result<ClientDto>.Failure("A client with this email already exists.", ErrorType.Conflict);
            }

            var identifier = request.Type.MapToDomainClientType() == ClientType.Individual ? request.PersonalIdentificationNumber : request.CompanyRegistrationNumber;

            if (!string.IsNullOrWhiteSpace(identifier) && await _unitOfWork.Clients.IdentificationNumberExistsAsync(identifier, cancellationToken))
            {
                _logger.LogWarning("Client creation failed. Identification number already exists. Identifier={Identifier}", identifier);
                return Result<ClientDto>.Failure("A client with this identification number already exists.", ErrorType.Conflict);
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                Address? address = null;
                if (request.Address != null)
                {
                    address = new Address(request.Address.Street, request.Address.Number);
                }

                var contactInfo = new ContactInfo(request.Email, request.Phone, address);

                var client = new Client(request.Type.MapToDomainClientType(), request.FullName, contactInfo, request.PersonalIdentificationNumber, request.CompanyRegistrationNumber);

                await _unitOfWork.Clients.AddAsync(client, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                _logger.LogInformation("Client created. ClientId={ClientId}, Type={Type}", client.Id, client.Type);

                ClientDto dto = client;

                return Result<ClientDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during client creation. Email={Email}", request.Email);
                return Result<ClientDto>.Failure(error: "Unexpected error during client creation.", ErrorType.Generic);
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(cancellationToken);
                    }
                    catch (Exception finalRbEx)
                    {
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed during client creation. Email={Email}", request.Email);
                    }
                }
            }
        }
    }
}