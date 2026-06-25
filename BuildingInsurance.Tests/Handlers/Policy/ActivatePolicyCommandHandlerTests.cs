using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Application.Features.Common.Models;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Policy
{
    public sealed class ActivatePolicyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<IClock> _clock = new();
        private readonly Mock<ILogger<ActivatePolicyCommandHandler>> _logger = new();
        private readonly Mock<IPolicyRepository> _policies = new();
        private readonly Mock<IBrokerVerifier> _brokerVerifier = new();
        private readonly Mock<IPolicyPricingService> _pricingService = new();
        private readonly Mock<IClientBuildingVerifier> _clientBuildingVerifier = new();
        private readonly Mock<ICurrencyVerifier> _currencyVerifier = new();
        private readonly ActivatePolicyCommandHandler _handler;

        public ActivatePolicyCommandHandlerTests()
        {
            _uow.SetupGet(x => x.Policies).Returns(_policies.Object);
            _clock.SetupGet(x => x.UtcNow).Returns(new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc));
            _currencyVerifier
                .Setup(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(new Domain.Entities.Metadata.Currency("EUR", "Euro", 1.0m, true)));

            _clientBuildingVerifier
                .Setup(v => v.GetAndVerifyAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success(
                    (Client: new Domain.Entities.Clients.Client(
                        id: Guid.NewGuid(),
                        type: ClientType.Individual,
                        fullName: "Client",
                        contactInfo: new ContactInfo("client@x.com", "0700000000", null),
                        personalIdentificationNumber: "1111111111111",
                        companyRegistrationNumber: null),
                     Building: new Domain.Entities.Buildings.Building(
                        id: Guid.NewGuid(),
                        clientId: Guid.NewGuid(),
                        address: new Address("Main", "1"),
                        cityId: Guid.NewGuid(),
                        constructionYear: 2000,
                        type: BuildingType.Residential,
                        numberOfFloors: 1,
                        surfaceArea: 80m,
                        insuredValue: 100_000m,
                        riskIndicators: RiskIndicators.None
                     )
                    )));

            _pricingService
                .Setup(s => s.CalculateAsync(It.IsAny<Domain.Entities.Policies.Policy>(), It.IsAny<Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PolicyPricingResult(FinalPremium: 150m, Fees: new List<AppliedFeeSnapshot>(), Risks: new List<AppliedRiskSnapshot>()));

            _handler = new ActivatePolicyCommandHandler(
                _uow.Object,
                _clock.Object,
                _logger.Object,
                _brokerVerifier.Object,
                 _pricingService.Object,
                _clientBuildingVerifier.Object,
                _currencyVerifier.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Policy_NotFound()
        {
            var policyId = Guid.NewGuid();

            _policies.Setup(r => r.GetByIdAsync(policyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Policies.Policy?)null);

            var result = await _handler.Handle(new ActivatePolicyCommand(policyId), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Policy with ID {policyId} not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_Policy_Already_Active()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(10),
                endDate: nowUtc.AddDays(40),
                basePremium: 100m);

            policy.SetPricing(150m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(150m);
            policy.Activate(nowUtc);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(PolicyStatus.Active, result.Value!.PolicyStatus);

            _brokerVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Broker_NotFound()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(10),
                endDate: nowUtc.AddDays(40),
                basePremium: 100m);

            policy.SetPricing(150m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker not found.", ErrorType.NotFound));

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Broker not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Broker_Inactive()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(10),
                endDate: nowUtc.AddDays(40),
                basePremium: 100m);

            policy.SetPricing(150m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker is inactive.", ErrorType.Validation));

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Broker is inactive.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_StartDate_Is_In_The_Past()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(-1),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);
            
            policy.SetPricing(finalPremium: 100m, appliedFees: Enumerable.Empty<PolicyAppliedFee>(), appliedRiskFactors: Enumerable.Empty<PolicyAppliedRiskFactor>());
            
            var broker = new Domain.Entities.Management.Broker(
                id: policy.BrokerId,
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@broker.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: null);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Success(broker));

            _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Start date should not be in the past.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_FinalPremium_NotCalculated()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(10),
                endDate: nowUtc.AddDays(40),
                basePremium: 100m);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier.Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Success(new Domain.Entities.Management.Broker(
                    id: policy.BrokerId,
                    brokerCode: "BR01",
                    name: "John Broker",
                    contactInfo: new ContactInfo("john@broker.com", "0700000000"),
                    brokerStatus: BrokerStatus.Active,
                    commissionPercentage: null)));

            _currencyVerifier.Setup(v => v.GetActiveAsync(policy.CurrencyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Metadata.Currency>.Success(new Domain.Entities.Metadata.Currency(policy.CurrencyId, "EUR", "Euro", 1m, true)));

            _clientBuildingVerifier.Setup(v => v.GetAndVerifyAsync(policy.ClientId, policy.BuildingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(Domain.Entities.Clients.Client, Domain.Entities.Buildings.Building)>.Success(
                    (new Domain.Entities.Clients.Client(policy.ClientId, ClientType.Individual, "John", new ContactInfo("a@a.com", "07"), "12345678901", null),
                     new Domain.Entities.Buildings.Building(policy.BuildingId, policy.ClientId, new Address("S", "1"), Guid.NewGuid(), 2000, BuildingType.Residential, 1, 50m, 100000m, RiskIndicators.None)
                    )));

            _pricingService.Setup(p => p.CalculateAsync(policy, It.IsAny<Domain.Entities.Buildings.Building>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PolicyPricingResult?)null);

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Policy pricing must be calculated before activation.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_AndCommit_When_Valid()
        {
            var nowUtc = _clock.Object.UtcNow;

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(10),
                endDate: nowUtc.AddDays(40),
                basePremium: 100m);

            policy.SetPricing(150m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());

            var broker = new Domain.Entities.Management.Broker(
                id: policy.BrokerId,
                brokerCode: "BR01",
                name: "John Broker",
                contactInfo: new ContactInfo("john@broker.com", "0700000000"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: null);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Success(broker));

            _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _handler.Handle(new ActivatePolicyCommand(policy.Id), CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(PolicyStatus.Active, result.Value!.PolicyStatus);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}