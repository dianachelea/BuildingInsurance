using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Clients;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers
{
    public sealed class ClientBuildingVerifier : IClientBuildingVerifier
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientBuildingVerifier> _logger;

        public ClientBuildingVerifier(IUnitOfWork unitOfWork, ILogger<ClientBuildingVerifier> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<(Client Client, Building Building)>>
            GetAndVerifyAsync(Guid clientId, Guid buildingId, CancellationToken cancellationToken)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId, cancellationToken);
            if (client is null)
            {
                _logger.LogDebug("Client with ID {ClientId} not found during verification.", clientId);
                return Result<(Client, Building)>.Failure($"Client with ID {clientId} not found.", ErrorType.NotFound);
            }

            var building = await _unitOfWork.Buildings.GetByIdAsync(buildingId, cancellationToken);
            if (building is null)
            {
                _logger.LogDebug("Building with ID {BuildingId} not found during verification.", buildingId);
                return Result<(Client, Building)>.Failure($"Building with ID {buildingId} not found.", ErrorType.NotFound);
            }

            if (building.ClientId != clientId)
            {
                _logger.LogDebug("Building {BuildingId} does not belong to Client {ClientId}.", buildingId, clientId);
                return Result<(Client, Building)>.Failure("Building does not belong to the given client.", ErrorType.Validation);
            }

            return Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building));
        }
    }
}