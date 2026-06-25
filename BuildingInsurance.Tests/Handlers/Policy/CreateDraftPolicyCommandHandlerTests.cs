using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Policy
{
    public sealed class CreateDraftPolicyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IPolicyRepository> _policies = new();
        private readonly Mock<IPolicyPricingService> _pricing = new();
        private readonly Mock<IClock> _clock = new();
        private readonly Mock<ILogger<CreateDraftPolicyCommandHandler>> _logger = new();
        private readonly CreateDraftPolicyCommandHandler _handler;
        private readonly Mock<IClientBuildingVerifier> _clientBuildingVerifier = new();
        private readonly Mock<ICurrencyVerifier> _currencyVerifier = new();
        private readonly Mock<IBrokerVerifier> _brokerVerifier = new();

        public CreateDraftPolicyCommandHandlerTests()
        {
            _uow.SetupGet(u => u.Policies).Returns(_policies.Object);

            _clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc));

            _handler = new CreateDraftPolicyCommandHandler(
                _uow.Object,
                _pricing.Object,
                _logger.Object,
                _clientBuildingVerifier.Object,
                _currencyVerifier.Object,
                _brokerVerifier.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Client_DoesNotExist()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client Client, Domain.Entities.Buildings.Building Building)>
                .Failure($"Client with ID {cmd.ClientId} not found.", ErrorType.NotFound));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Client with ID {cmd.ClientId} not found.", result.Error);

            _clientBuildingVerifier.Verify(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()), Times.Once);
            _currencyVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _brokerVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            _policies.Verify(r => r.HasOverlappingActivePolicyAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
            _pricing.Verify(p => p.CalculateAsync(It.IsAny<Domain.Entities.Policies.Policy>(), It.IsAny<Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()), Times.Never);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Building_DoesNotExist()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client Client, Domain.Entities.Buildings.Building Building)>
                .Failure($"Building with ID {cmd.BuildingId} not found.", ErrorType.NotFound));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Building with ID {cmd.BuildingId} not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Building_DoesNotBelong_To_Client()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client Client, Domain.Entities.Buildings.Building Building)>
                .Failure("Building does not belong to the given client.", ErrorType.Validation));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Building does not belong to the given client.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Currency_DoesNotExist()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                cmd.ClientId, 
                ClientType.Individual, 
                "John Doe", 
                new ContactInfo("john@email.com", "0700000000"), 
                "12345678901");

            var building = new Domain.Entities.Buildings.Building(
                cmd.BuildingId,
                cmd.ClientId,
                new Address("Main Street", "10"),
                Guid.NewGuid(),
                2005,
                BuildingType.Residential,
                2,
                120m,
                150000m,
                RiskIndicators.FloodZone);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            _currencyVerifier
                .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Failure("Currency not found.", ErrorType.NotFound));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Currency not found.", result.Error);

            _brokerVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Currency_IsInactive()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                cmd.ClientId,
                ClientType.Individual,
                "John Doe",
                new ContactInfo("john@email.com", "0700000000"),
                "12345678901");

            var building = new Domain.Entities.Buildings.Building(
                cmd.BuildingId,
                cmd.ClientId,
                new Address("Main Street", "10"),
                Guid.NewGuid(),
                2005,
                BuildingType.Residential,
                2,
                120m,
                150000m,
                RiskIndicators.FloodZone);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            _currencyVerifier
               .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Failure("Currency is inactive.", ErrorType.Validation));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Currency is inactive.", result.Error);

            _brokerVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Broker_NotFound()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                cmd.ClientId,
                ClientType.Individual,
                "John Doe",
                new ContactInfo("john@email.com", "0700000000"),
                "12345678901");

            var building = new Domain.Entities.Buildings.Building(
                cmd.BuildingId,
                cmd.ClientId,
                new Address("Main Street", "10"),
                Guid.NewGuid(),
                2005,
                BuildingType.Residential,
                2,
                120m,
                150000m,
                RiskIndicators.FloodZone);

            var currency = new Domain.Entities.Metadata.Currency(
                cmd.CurrencyId,
                "EUR",
                "Euro",
                exchangeRateToBase: 1m,
                isActive: true);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            _currencyVerifier
                .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(currency));

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker not found.", ErrorType.NotFound));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Broker not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Broker_IsInactive()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                cmd.ClientId,
                ClientType.Individual,
                "John Doe",
                new ContactInfo("john@email.com", "0700000000"),
                "12345678901");

            var building = new Domain.Entities.Buildings.Building(
                cmd.BuildingId,
                cmd.ClientId,
                new Address("Main Street", "10"),
                Guid.NewGuid(),
                2005,
                BuildingType.Residential,
                2,
                120m,
                150000m,
                RiskIndicators.FloodZone);

            var currency = new Domain.Entities.Metadata.Currency(
                cmd.CurrencyId,
                "EUR",
                "Euro",
                exchangeRateToBase: 1m,
                isActive: true);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            _currencyVerifier
                .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(currency));

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker is inactive.", ErrorType.Validation));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Broker is inactive.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_When_OverlappingPolicy_Exists()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                cmd.ClientId,
                ClientType.Individual,
                "John Doe",
                new ContactInfo("john@email.com", "0700000000"),
                "12345678901");

            var building = new Domain.Entities.Buildings.Building(
                cmd.BuildingId,
                cmd.ClientId,
                new Address("Main Street", "10"),
                Guid.NewGuid(),
                2005,
                BuildingType.Residential,
                2,
                120m,
                150000m,
                RiskIndicators.FloodZone);

            var currency = new Domain.Entities.Metadata.Currency(
                cmd.CurrencyId,
                "EUR",
                "Euro",
                exchangeRateToBase: 1m,
                isActive: true);

            var broker = new Domain.Entities.Management.Broker(
                cmd.BrokerId,
                "BR01",
                "John Broker",
                new ContactInfo("john@broker.com", "0700000000"),
                BrokerStatus.Active,
                commissionPercentage: null);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success((client, building)));

            _currencyVerifier
                .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(currency));

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Success(broker));

            _policies
                .Setup(r => r.HasOverlappingActivePolicyAsync(cmd.BuildingId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Building already has an overlapping active policy.", result.Error);

            _pricing.Verify(p => p.CalculateAsync(It.IsAny<Domain.Entities.Policies.Policy>(), It.IsAny<Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()), Times.Never);
            _policies.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Policies.Policy>(), It.IsAny<CancellationToken>()), Times.Never);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_Input_Is_Valid()
        {
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(10),
                EndDate = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc).AddDays(20),
                BasePremium = 100m
            };

            var client = new Domain.Entities.Clients.Client(
                id: cmd.ClientId,
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo("john@email.com", "0700000000"),
                personalIdentificationNumber: "12345678901",
                companyRegistrationNumber: null);

            var building = new Domain.Entities.Buildings.Building(
                id: cmd.BuildingId,
                clientId: cmd.ClientId,
                address: new Address("Main Street", "10"),
                cityId: Guid.NewGuid(),
                constructionYear: 2005,
                type: BuildingType.Residential,
                numberOfFloors: 2,
                surfaceArea: 120m,
                insuredValue: 150000m,
                riskIndicators: RiskIndicators.FloodZone);

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client Client, Domain.Entities.Buildings.Building Building)>.Success((client, building)));

            var currency = new Domain.Entities.Metadata.Currency(
                id: cmd.CurrencyId,
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 1m,
                isActive: true);

            _currencyVerifier
                .Setup(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(currency));

            var broker = new Domain.Entities.Management.Broker(
                id: cmd.BrokerId,
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@broker.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: null);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(cmd.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Success(broker));

            _policies
                .Setup(r => r.HasOverlappingActivePolicyAsync(cmd.BuildingId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var pricing = new PolicyPricingResult(
                FinalPremium: 123m,
                Fees: new List<AppliedFeeSnapshot>
                { new AppliedFeeSnapshot(Guid.NewGuid(), "AdminFee", 0.10m) },
                Risks: new List<AppliedRiskSnapshot>
                { new AppliedRiskSnapshot(Guid.NewGuid(), RiskFactorLevel.City, Guid.NewGuid(), null, 0.20m) });

            _pricing
                .Setup(p => p.CalculateAsync(It.IsAny<Domain.Entities.Policies.Policy>(), building, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pricing);

            _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(cmd.ClientId, result.Value!.ClientId);
            Assert.Equal(cmd.BuildingId, result.Value.BuildingId);
            Assert.Equal(cmd.BrokerId, result.Value.BrokerId);
            Assert.Equal(cmd.CurrencyId, result.Value.CurrencyId);
            Assert.Equal(PolicyStatus.Draft, result.Value.PolicyStatus);
            Assert.Equal(0m, result.Value.FinalPremium);
            Assert.Equal(123m, result.Value.EstimatedFinalPremium);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            _policies.Verify(r => r.AddAsync(It.Is<Domain.Entities.Policies.Policy>(p =>
                p.ClientId == cmd.ClientId &&
                p.BuildingId == cmd.BuildingId &&
                p.BrokerId == cmd.BrokerId &&
                p.CurrencyId == cmd.CurrencyId &&
                p.PolicyStatus == PolicyStatus.Draft
            ), It.IsAny<CancellationToken>()), Times.Once);

            _clientBuildingVerifier.Verify(v => v.GetAndVerifyAsync(cmd.ClientId, cmd.BuildingId, It.IsAny<CancellationToken>()), Times.Once);
            _currencyVerifier.Verify(v => v.GetActiveAsync(cmd.CurrencyId, It.IsAny<CancellationToken>()), Times.Once);
            _brokerVerifier.Verify(v => v.GetActiveAsync(cmd.BrokerId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}