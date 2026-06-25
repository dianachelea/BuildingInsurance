using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration;
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
    public sealed class CreateRiskFactorConfigurationCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRiskFactorConfigurationRepository> _riskRepoMock;
        private readonly Mock<IRiskFactorTargetVerifier> _targetValidatorMock;
        private readonly Mock<ILogger<CreateRiskFactorConfigurationCommandHandler>> _loggerMock;

        private readonly CreateRiskFactorConfigurationCommandHandler _handler;

        public CreateRiskFactorConfigurationCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _riskRepoMock = new Mock<IRiskFactorConfigurationRepository>();
            _targetValidatorMock = new Mock<IRiskFactorTargetVerifier>();
            _loggerMock = new Mock<ILogger<CreateRiskFactorConfigurationCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.RiskFactorConfigurations).Returns(_riskRepoMock.Object);

            _handler = new CreateRiskFactorConfigurationCommandHandler(
                _unitOfWorkMock.Object,
                _targetValidatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_When_TargetValidation_Returns_Result()
        {
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null,
                AdjustmentPercentage = 0.15m,
                IsActive = true
            };

            var validation = Result<RiskFactorConfigurationDto>
                .Failure("Target not found.", ErrorType.NotFound);

            _targetValidatorMock
                .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(validation);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Target not found.", result.Error);

            _riskRepoMock.Verify(r => r.GetByTargetAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid?>(), It.IsAny<BuildingType?>(), It.IsAny<CancellationToken>()), Times.Never);
            _riskRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Metadata.RiskFactorConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_When_ConfigAlreadyExists_For_Target()
        {
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.BuildingType,
                ReferenceId = null,
                BuildingType = BuildingTypeContract.Residential,
                AdjustmentPercentage = 0.10m,
                IsActive = true
            };

            _targetValidatorMock
                .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _riskRepoMock
                .Setup(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Domain.Entities.Metadata.RiskFactorConfiguration(
                    cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), cmd.AdjustmentPercentage, cmd.IsActive));

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Risk factor configuration already exists for the provided target.", result.Error);

            _riskRepoMock.Verify(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()), Times.Once);

            _riskRepoMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Metadata.RiskFactorConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_When_Input_Is_Valid()
        {
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null,
                AdjustmentPercentage = 0.25m,
                IsActive = true
            };

            _targetValidatorMock
                .Setup(v => v.VerifyExistsAsync<RiskFactorConfigurationDto>(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result<RiskFactorConfigurationDto>?)null);

            _riskRepoMock
                .Setup(r => r.GetByTargetAsync(cmd.Level.MapToDomainRiskFactorLevel(), cmd.ReferenceId, cmd.BuildingType.MapToDomainBuildingTypeOptional(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Metadata.RiskFactorConfiguration?)null);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.NotEqual(Guid.Empty, result.Value!.Id);
            Assert.Equal(cmd.Level.MapToDomainRiskFactorLevel(), result.Value.Level);
            Assert.Equal(cmd.ReferenceId, result.Value.ReferenceId);
            Assert.Equal(cmd.BuildingType.MapToDomainBuildingTypeOptional(), result.Value.BuildingType);
            Assert.Equal(cmd.AdjustmentPercentage, result.Value.AdjustmentPercentage);
            Assert.Equal(cmd.IsActive, result.Value.IsActive);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

            _riskRepoMock.Verify(r => r.AddAsync(It.Is<Domain.Entities.Metadata.RiskFactorConfiguration>(e =>
                e.Level == cmd.Level.MapToDomainRiskFactorLevel() &&
                e.ReferenceId == cmd.ReferenceId &&
                e.BuildingType == cmd.BuildingType.MapToDomainBuildingTypeOptional() &&
                e.AdjustmentPercentage == cmd.AdjustmentPercentage &&
                e.IsActive == cmd.IsActive
            ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}