using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace BuildingInsurance.Tests.Handlers.FeeConfiguration
{
    public sealed class CreateFeeConfigurationCommandHandlerTests
    {
        [Fact]
        public async Task Should_Return_Conflict_When_Overlapping_Exists()
        {
            var uow = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<CreateFeeConfigurationCommandHandler>>();

            uow.Setup(x => x.FeeConfigurations.ExistsOverlappingAsync(
                    FeeType.AdminFee,
                    RiskIndicators.None,
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>(),
                    null))
                .ReturnsAsync(true);

            var handler = new CreateFeeConfigurationCommandHandler(uow.Object, logger.Object);

            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Admin Fee",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.1m,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "Another fee configuration exists with overlapping period for the same type and risk indicators.",
                result.Error
            );

            uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Should_Create_FeeConfiguration_When_No_Overlap()
        {
            var uow = new Mock<IUnitOfWork>();
            var logger = new Mock<ILogger<CreateFeeConfigurationCommandHandler>>();

            uow.Setup(x => x.FeeConfigurations.ExistsOverlappingAsync(
                    It.IsAny<FeeType>(),
                    It.IsAny<RiskIndicators>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>(),
                    null))
                .ReturnsAsync(false);

            uow.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            uow.Setup(x => x.FeeConfigurations.AddAsync(
                    It.IsAny<BuildingInsurance.Domain.Entities.Metadata.FeeConfiguration>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateFeeConfigurationCommandHandler(uow.Object, logger.Object);

            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Admin Fee",
                FeeType = FeeTypeContract.AdminFee,
                FeePercentage = 0.1m,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                IsActive = true,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Admin Fee", result.Value.Name);
            Assert.Equal(FeeType.AdminFee, result.Value.FeeType);

            uow.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}