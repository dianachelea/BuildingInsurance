using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Common;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.RiskFactorConfiguration
{
    public sealed class UpdateRiskFactorConfigurationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRiskFactorConfigurationRepository> _riskFactorRepoMock;
        private readonly Mock<IPolicyRepository> _policyRepoMock;
        private readonly Mock<IRiskFactorTargetVerifier> _validatorMock;
        private readonly Mock<ILogger<UpdateRiskFactorConfigurationCommandHandler>> _loggerMock;

        private readonly UpdateRiskFactorConfigurationCommandHandler _handler;

        public UpdateRiskFactorConfigurationCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _riskFactorRepoMock = new Mock<IRiskFactorConfigurationRepository>();
            _policyRepoMock = new Mock<IPolicyRepository>();
            _validatorMock = new Mock<IRiskFactorTargetVerifier>();
            _loggerMock = new Mock<ILogger<UpdateRiskFactorConfigurationCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.RiskFactorConfigurations).Returns(_riskFactorRepoMock.Object);
            _unitOfWorkMock.SetupGet(u => u.Policies).Returns(_policyRepoMock.Object);

            _handler = new UpdateRiskFactorConfigurationCommandHandler(
                _unitOfWorkMock.Object,
                _validatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenRiskFactorDoesNotExist()
        {
            var id = Guid.NewGuid();

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null,
                AdjustmentPercentage = 0.10m,
                IsActive = true
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.RiskFactorConfiguration?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Risk factor configuration with ID {id} not found.", result.Error);

            _policyRepoMock.Verify(p => p.IsRiskFactorUsedInActivePoliciesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _validatorMock.Verify(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnValidationResult_WhenValidatorReturnsFailure()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = rf.ReferenceId,
                BuildingType = null,
                AdjustmentPercentage = rf.AdjustmentPercentage,
                IsActive = rf.IsActive
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            var validationFailure = Result<RiskFactorConfigurationDto>
                .Failure("Validation failed.", ErrorType.Conflict);

            _validatorMock
                .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(
                It.IsAny<IRiskFactorTargetRequest>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationFailure);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Validation failed.", result.Error);

            _policyRepoMock.Verify(p => p.IsRiskFactorUsedInActivePoliciesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenUsedInActivePolicies_AndAttemptToDeactivate()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = rf.ReferenceId,
                BuildingType = null,
                AdjustmentPercentage = rf.AdjustmentPercentage,
                IsActive = false
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);
            
            _validatorMock
                .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(
                    It.IsAny<IRiskFactorTargetRequest>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration is used by active policies and cannot be updated. No changes were applied. Create a new risk factor configuration for future use.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenUsedInActivePolicies_AndPricingFieldsChanged()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = rf.ReferenceId,
                BuildingType = null,
                AdjustmentPercentage = rf.AdjustmentPercentage + 0.05m,
                IsActive = rf.IsActive
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            _validatorMock
               .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration is used by active policies and cannot be updated. No changes were applied. Create a new risk factor configuration for future use.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccessDto_WhenUsedInActivePolicies_AndNoPricingFieldsChanged()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = rf.ReferenceId,
                BuildingType = null,
                AdjustmentPercentage = rf.AdjustmentPercentage,
                IsActive = rf.IsActive
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            _validatorMock
               .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.StartsWith("Risk factor configuration is used by active policies", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenNotUsedInActivePolicies_ButTargetAlreadyExists()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var other = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.05m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.City,
                ReferenceId = other.ReferenceId,
                BuildingType = null,
                AdjustmentPercentage = 0.12m,
                IsActive = true
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            _validatorMock
               .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _riskFactorRepoMock
                .Setup(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(other);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration already exists for the provided target.", result.Error);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenNotUsedInActivePolicies_ValidUpdate_AndCommits()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.10m,
                isActive: false);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.County,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null,
                AdjustmentPercentage = 0.20m,
                IsActive = true
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            _validatorMock
               .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _riskFactorRepoMock
                .Setup(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.RiskFactorConfiguration?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(rf.Id, result.Value!.Id);
            Assert.Equal(cmd.Level.MapToDomainRiskFactorLevel(), result.Value.Level);
            Assert.Equal(cmd.ReferenceId, result.Value.ReferenceId);
            Assert.Equal(cmd.BuildingType.MapToDomainBuildingTypeOptional(), result.Value.BuildingType);
            Assert.Equal(cmd.AdjustmentPercentage, result.Value.AdjustmentPercentage);
            Assert.Equal(cmd.IsActive, result.Value.IsActive);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _riskFactorRepoMock.Verify(r => r.Update(rf), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenNotUsedInActivePolicies_ValidUpdate_BuildingTypeLevel()
        {
            var rf = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.BuildingType,
                referenceId: null,
                buildingType: BuildingType.Residential,
                adjustmentPercentage: 0.10m,
                isActive: true);

            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = rf.Id,
                Level = RiskFactorLevelContract.BuildingType,
                ReferenceId = null,
                BuildingType = BuildingTypeContract.Office,
                AdjustmentPercentage = 0.20m,
                IsActive = true
            };

            _riskFactorRepoMock
                .Setup(r => r.GetByIdAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rf);

            _validatorMock
               .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(It.IsAny<IRiskFactorTargetRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _policyRepoMock
                .Setup(p => p.IsRiskFactorUsedInActivePoliciesAsync(rf.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _riskFactorRepoMock
                .Setup(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.RiskFactorConfiguration?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _riskFactorRepoMock.Verify(r => r.Update(rf), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}