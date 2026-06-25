using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.GetFeeConfigurationById;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using Moq;

namespace BuildingInsurance.Tests.Handlers.FeeConfiguration
{
    public sealed class GetFeeConfigurationByIdHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFeeConfigurationRepository> _feeRepoMock;
        private readonly GetFeeConfigurationByIdHandler _handler;

        public GetFeeConfigurationByIdHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _feeRepoMock = new Mock<IFeeConfigurationRepository>();

            _unitOfWorkMock.SetupGet(u => u.FeeConfigurations).Returns(_feeRepoMock.Object);

            _handler = new GetFeeConfigurationByIdHandler(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenFeeDoesNotExist()
        {
            var id = Guid.NewGuid();
            var query = new GetFeeConfigurationByIdQuery(id);

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(id, CancellationToken.None))
                .ReturnsAsync((Domain.Entities.Metadata.FeeConfiguration?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Fee configuration not found.", result.Error);

            _feeRepoMock.Verify(r => r.GetByIdAsync(id, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenFeeExists()
        {
            var fee = new Domain.Entities.Metadata.FeeConfiguration(
                "Admin Fee",
                FeeType.AdminFee,
                0.10m,
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(30),
                true,
                RiskIndicators.None);

            var query = new GetFeeConfigurationByIdQuery(fee.Id);

            _feeRepoMock
                .Setup(r => r.GetByIdAsync(fee.Id, CancellationToken.None))
                .ReturnsAsync(fee);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(fee.Id, result.Value!.Id);
            Assert.Equal("Admin Fee", result.Value.Name);
            Assert.Equal(FeeType.AdminFee, result.Value.FeeType);
            Assert.Equal(0.10m, result.Value.FeePercentage);
            Assert.True(result.Value.IsActive);
            Assert.Equal(RiskIndicators.None, result.Value.RiskIndicators);

            _feeRepoMock.Verify(r => r.GetByIdAsync(fee.Id, CancellationToken.None), Times.Once);
        }
    }
}