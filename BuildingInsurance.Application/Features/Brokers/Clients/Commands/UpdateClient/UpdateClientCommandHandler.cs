using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateClientCommandHandler> _logger;

        public UpdateClientCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateClientCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(request.ClientId, cancellationToken);
            if (client is null)
            {
                _logger.LogWarning("Client update failed. Client not found. ClientId={ClientId}", request.ClientId);
                return Result<ClientDto>.Failure("Client not found.", ErrorType.NotFound);
            }

            string? normalizedIdentifier = null;
            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                normalizedIdentifier = request.IdentificationNumber.Trim();

                var existsForOther = await _unitOfWork.Clients
                    .IdentificationNumberExistsForOtherClientAsync(client.Id, normalizedIdentifier, cancellationToken);

                if (existsForOther)
                {
                    _logger.LogWarning("Client update failed. Identification number conflict. ClientId={ClientId}, Identifier={Identifier}",
                        client.Id, normalizedIdentifier);

                    return Result<ClientDto>.Failure("Identification number already exists.", ErrorType.Conflict);
                }
            }

            string? normalizedEmail = null;
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                normalizedEmail = request.Email.Trim().ToLowerInvariant();

                var emailExistsForOther = await _unitOfWork.Clients
                    .EmailExistsForOtherClientAsync(client.Id, normalizedEmail, cancellationToken);

                if (emailExistsForOther)
                {
                    _logger.LogWarning("Client update failed. Email conflict. ClientId={ClientId}, Email={Email}",
                        client.Id, normalizedEmail);

                    return Result<ClientDto>.Failure("Email already exists.", ErrorType.Conflict);
                }
            }

            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;

                client.UpdateFullName(request.FullName);

                Address? address = null;

                if (request.Address is not null)
                    address = new Address(request.Address.Street, request.Address.Number);

                var contactInfo = new ContactInfo(request.Email, request.Phone, address);
                client.UpdateContactInfo(contactInfo);

                if (normalizedIdentifier is not null)
                {
                    client.ChangeIdentifier(normalizedIdentifier, request.IdentificationChangeReason!);
                    _logger.LogWarning("Client identifier changed. ClientId={ClientId}, Reason={Reason}", client.Id, request.IdentificationChangeReason);
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                committed = true;

                _logger.LogInformation("Client successfully updated. ClientId={ClientId}", client.Id);
                ClientDto dto = client;
                return Result<ClientDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during client update. ClientId={ClientId}", request.ClientId);
                return Result<ClientDto>.Failure("Unexpected error during client update.", ErrorType.Generic);
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
                        _logger.LogWarning(finalRbEx, "Final rollback attempt failed for ClientId = {ClientId}", request.ClientId);
                    }
                }
            }
        }
    }
}