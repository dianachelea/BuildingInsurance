using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.GetRiskFactorConfigurationById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using Moq;

namespace BuildingInsurance.Tests.Handlers.RiskFactorConfiguration
{
    public sealed class GetRiskFactorConfigurationByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRiskFactorConfigurationRepository> _repoMock;
        private readonly GetRiskFactorConfigurationByIdHandler _handler;

        public GetRiskFactorConfigurationByIdHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _repoMock = new Mock<IRiskFactorConfigurationRepository>();

            _unitOfWorkMock.SetupGet(u => u.RiskFactorConfigurations).Returns(_repoMock.Object);

            _handler = new GetRiskFactorConfigurationByIdHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenConfigDoesNotExist()
        {
            var id = Guid.NewGuid();
            var query = new GetRiskFactorConfigurationByIdQuery(id);

            _repoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Domain.Entities.Metadata.RiskFactorConfiguration?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Risk factor configuration not found.", result.Error);

            _repoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenConfigExists()
        {
            var entity = new Domain.Entities.Metadata.RiskFactorConfiguration(
                level: RiskFactorLevel.City,
                referenceId: Guid.NewGuid(),
                buildingType: null,
                adjustmentPercentage: 0.08m,
                isActive: true);

            var query = new GetRiskFactorConfigurationByIdQuery(entity.Id);

            _repoMock
                .Setup(r => r.GetByIdAsync(entity.Id, CancellationToken.None))
                .ReturnsAsync(entity);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(entity.Id, result.Value!.Id);
            Assert.Equal(entity.Level, result.Value.Level);
            Assert.Equal(entity.ReferenceId, result.Value.ReferenceId);
            Assert.Equal(entity.BuildingType, result.Value.BuildingType);
            Assert.Equal(entity.AdjustmentPercentage, result.Value.AdjustmentPercentage);
            Assert.Equal(entity.IsActive, result.Value.IsActive);

            _repoMock.Verify(r => r.GetByIdAsync(entity.Id, CancellationToken.None), Times.Once);
        }
    }
}