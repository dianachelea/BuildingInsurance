using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.FeeConfiguration
{
    public sealed class UpdateFeeConfigurationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFeeConfigurationRepository> _feeRepoMock;
        private readonly Mock<IPolicyRepository> _policyRepoMock;
        private readonly Mock<ILogger<UpdateFeeConfigurationCommandHandler>> _loggerMock;

        private readonly UpdateFeeConfigurationCommandHandler _handler;

        public UpdateFeeConfigurationCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _feeRepoMock = new Mock<IFeeConfigurationRepository>();
            _policyRepoMock = new Mock<IPolicyRepository>();
            _loggerMock = new Mock<ILogger<UpdateFeeConfigurationCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.FeeConfigurations).Returns(_feeRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.Policies).Returns(_policyRepoMock.Object);

            _handler = new UpdateFeeConfigurationCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenFeeDoesNotExist()
        {
            var id = Guid.NewGuid();

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = id,
                Name = "Updated",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.2m,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BuildingInsurance.Domain.Entities.Metadata.FeeConfiguration?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Fee configuration with ID {id} not found.", result.Error);

            _feeRepoMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            _policyRepoMock.Verify(r => r.IsFeeUsedInActivePoliciesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenNotUsedInActivePolicies_ButOverlapsExist()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: DateTime.UtcNow.AddDays(-1),
                effectiveTo: DateTime.UtcNow.AddDays(30),
                isActive: true,
                riskIndicators: RiskIndicators.None
            );

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = fee.Id,
                Name = "Updated",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.2m,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _policyRepoMock
                .Setup(r => r.IsFeeUsedInActivePoliciesAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _feeRepoMock
                .Setup(r => r.ExistsOverlappingAsync(
                    cmd.FeeType.MapToDomainFeeType(),
                    cmd.RiskIndicators.MapToDomainRiskIndicators(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>(),
                    fee.Id))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Another fee configuration exists with overlapping period for the same type and risk indicators.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenNotUsedInActivePolicies_ValidUpdate()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: DateTime.UtcNow.AddDays(-1),
                effectiveTo: DateTime.UtcNow.AddDays(30),
                isActive: false,
                riskIndicators: RiskIndicators.None
            );

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = fee.Id,
                Name = "Updated Name",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.25m,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddDays(30),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _policyRepoMock
                .Setup(r => r.IsFeeUsedInActivePoliciesAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _feeRepoMock
                .Setup(r => r.ExistsOverlappingAsync(
                    cmd.FeeType.MapToDomainFeeType(),
                    cmd.RiskIndicators.MapToDomainRiskIndicators(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>(),
                    fee.Id))
                .ReturnsAsync(false);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.Equal(fee.Id, result.Value!.Id);
            Assert.Equal("Updated Name", result.Value.Name);
            Assert.Equal(FeeType.AdminFee, result.Value.FeeType);
            Assert.Equal(0.25m, result.Value.FeePercentage);
            Assert.True(result.Value.IsActive);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _feeRepoMock.Verify(r => r.Update(fee), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenUsedInActivePolicies_AndAttemptToDeactivate()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee", 
                feeType: FeeTypeContract.AdminFee.MapToDomainFeeType(), 
                feePercentage: 0.10m, 
                effectiveFrom: DateTime.UtcNow.AddDays(-1), 
                effectiveTo: DateTime.UtcNow.AddDays(30), 
                isActive: true, 
                riskIndicators: RiskIndicatorsContract.None.MapToDomainRiskIndicators());

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = fee.Id,
                Name = fee.Name,
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = fee.FeePercentage,
                EffectiveFrom = fee.EffectiveFrom,
                EffectiveTo = fee.EffectiveTo,
                IsActive = false,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _policyRepoMock
                .Setup(r => r.IsFeeUsedInActivePoliciesAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Cannot deactivate fee configuration because it is referenced by active policies.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenUsedInActivePolicies_AndPricingFieldsChanged()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: DateTime.UtcNow.AddDays(-1),
                effectiveTo: DateTime.UtcNow.AddDays(30),
                isActive: true,
                riskIndicators: RiskIndicators.None
            );

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = fee.Id,
                Name = fee.Name,
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = fee.FeePercentage + 0.01m,
                EffectiveFrom = fee.EffectiveFrom,
                EffectiveTo = fee.EffectiveTo,
                IsActive = fee.IsActive,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _policyRepoMock
                .Setup(r => r.IsFeeUsedInActivePoliciesAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Cannot change percentage/type/effective period/risk indicators or deactivate because the fee is used by active policies. " +
                "You may change the name only, or create a new fee configuration for future periods.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUsedInActivePolicies_OnlyNameChanges()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                feeName: "Admin Fee",
                feeType: FeeType.AdminFee,
                feePercentage: 0.10m,
                effectiveFrom: DateTime.UtcNow.AddDays(-1),
                effectiveTo: DateTime.UtcNow.AddDays(30),
                isActive: true,
                riskIndicators: RiskIndicators.None
            );

            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = fee.Id,
                Name = "Renamed Fee",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = fee.FeePercentage,
                EffectiveFrom = fee.EffectiveFrom,
                EffectiveTo = fee.EffectiveTo,
                IsActive = fee.IsActive,
                RiskIndicators = RiskIndicatorsContract.None
            };

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(fee);

            _policyRepoMock
                .Setup(r => r.IsFeeUsedInActivePoliciesAsync(fee.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(fee.Id, result.Value!.Id);
            Assert.Equal("Renamed Fee", result.Value.Name);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _feeRepoMock.Verify(r => r.Update(fee), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}