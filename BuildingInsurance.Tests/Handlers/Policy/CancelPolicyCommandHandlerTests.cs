using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy;
using BuildingInsurance.Application.Features.Brokers.Policies.Common.Verifiers.Abstractions;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Policy
{
    public sealed class CancelPolicyCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _uow = new();
        private readonly Mock<ILogger<CancelPolicyCommandHandler>> _logger = new();
        private readonly Mock<IPolicyRepository> _policies = new();
        private readonly CancelPolicyCommandHandler _handler;
        private readonly Mock<IBrokerVerifier> _brokerVerifier = new();

        public CancelPolicyCommandHandlerTests()
        {
            _uow.SetupGet(x => x.Policies).Returns(_policies.Object);

            _handler = new CancelPolicyCommandHandler(_uow.Object, _logger.Object, _brokerVerifier.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Policy_NotFound()
        {
            var policyId = Guid.NewGuid();

            _policies.Setup(r => r.GetByIdAsync(policyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Policies.Policy?)null);

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policyId,
                Reason = "Customer request",
                CancellationEffectiveDate = new DateTime(2026, 03, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Policy with ID {policyId} not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_Policy_Already_Cancelled()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);
            policy.Cancel("Customer request", policy.StartDate.AddDays(1));

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                CancellationEffectiveDate = policy.StartDate.AddDays(1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(PolicyStatus.Cancelled, result.Value!.PolicyStatus);

            _brokerVerifier.Verify(v => v.GetActiveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Reason_Is_Invalid()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);

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

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Not allowed reason",
                CancellationEffectiveDate = policy.StartDate.AddDays(1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Policy cannot be cancelled due to invalid state or reason.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_CancellationDate_Before_StartDate()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);

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

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                CancellationEffectiveDate = policy.StartDate.AddDays(-1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Cancellation effective date cannot be before StartDate.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_When_Broker_NotFound()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker not found.", ErrorType.NotFound));

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                CancellationEffectiveDate = policy.StartDate.AddDays(1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Broker not found.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidation_When_Broker_Inactive()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);

            _policies.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(policy);

            _brokerVerifier
                .Setup(v => v.GetActiveAsync(policy.BrokerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Entities.Management.Broker>.Failure("Broker is inactive.", ErrorType.Validation));

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                CancellationEffectiveDate = policy.StartDate.AddDays(1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Broker is inactive.", result.Error);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_AndCommit_When_Valid()
        {
            var nowUtc = new DateTime(2026, 02, 05, 10, 0, 0, DateTimeKind.Utc);

            var policy = Domain.Entities.Policies.Policy.CreateDraft(
                clientId: Guid.NewGuid(),
                buildingId: Guid.NewGuid(),
                brokerId: Guid.NewGuid(),
                currencyId: Guid.NewGuid(),
                startDate: nowUtc.AddDays(5),
                endDate: nowUtc.AddDays(30),
                basePremium: 100m);

            policy.SetPricing(100m, Enumerable.Empty<PolicyAppliedFee>(), Enumerable.Empty<PolicyAppliedRiskFactor>());
            policy.SetFinalPremiumInBaseCurrency(100m);
            policy.Activate(nowUtc);

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

            var cmd = new CancelPolicyCommand
            {
                PolicyId = policy.Id,
                Reason = "Customer request",
                CancellationEffectiveDate = policy.StartDate.AddDays(1)
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(PolicyStatus.Cancelled, result.Value!.PolicyStatus);
            Assert.Equal(policy.StartDate.AddDays(1), result.Value.CancellationEffectiveDate);

            _uow.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _uow.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}