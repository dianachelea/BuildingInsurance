using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;

namespace BuildingInsurance.Tests.Handlers.Building
{
	public class ListBuildingsByClientHandlerTests
	{
		private readonly Mock<IBuildingRepository> _buildingRepositoryMock;
		private readonly ListBuildingsByClientHandler _handler;

		public ListBuildingsByClientHandlerTests()
		{
			_buildingRepositoryMock = new Mock<IBuildingRepository>();
			_handler = new ListBuildingsByClientHandler(_buildingRepositoryMock.Object);
		}

		[Fact]
		public async Task Handle_ShouldReturnEmpty_WhenNoBuildingsFound()
		{
			var clientId = Guid.NewGuid();
			var query = new ListBuildingsByClientQuery{ ClientId = clientId, Page = 1, PageSize = 10 };

			_buildingRepositoryMock
				.Setup(r => r.GetByClientIdPagedAsync(clientId, 1, 10, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Array.Empty<Domain.Entities.Buildings.Building>(), 0));

			var result = await _handler.Handle(query, CancellationToken.None);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			Assert.Empty(result.Value!.Items);
			Assert.Equal(0, result.Value.TotalCount);
			Assert.Equal(0, result.Value.TotalPages);

			_buildingRepositoryMock.Verify(r => r.GetByClientIdPagedAsync(clientId, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task Handle_ShouldReturnEmpty_WhenPageIsGreaterThanTotalPages()
		{
			var clientId = Guid.NewGuid();
			var query = new ListBuildingsByClientQuery { ClientId = clientId, Page = 3, PageSize = 2 };

			_buildingRepositoryMock
				.Setup(r => r.GetByClientIdPagedAsync(clientId, 3, 2, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Array.Empty<Domain.Entities.Buildings.Building>(), 3));

			var result = await _handler.Handle(query, CancellationToken.None);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			Assert.Empty(result.Value!.Items);
			Assert.Equal(3, result.Value.TotalCount);
			Assert.Equal(2, result.Value.TotalPages);

			_buildingRepositoryMock.Verify(r => r.GetByClientIdPagedAsync(clientId, 3, 2, It.IsAny<CancellationToken>()), Times.Once);
		}

		[Fact]
		public async Task Handle_ShouldReturnPagedItems_Correctly()
		{
			var clientId = Guid.NewGuid();
			var query = new ListBuildingsByClientQuery { ClientId = clientId, Page = 2, PageSize = 2 };

			var address3 = new Address("C", "3");
			var address4 = new Address("D", "4");

			var r = new RiskIndicators();

			var b3 = new Domain.Entities.Buildings.Building(Guid.NewGuid(), clientId, address3, Guid.NewGuid(), 1992, BuildingType.Industrial, 3, 70m, 70_000m, r);
			var b4 = new Domain.Entities.Buildings.Building(Guid.NewGuid(), clientId, address4, Guid.NewGuid(), 1993, BuildingType.Industrial, 4, 80m, 80_000m, r);

			_buildingRepositoryMock
				.Setup(rp => rp.GetByClientIdPagedAsync(clientId, 2, 2, It.IsAny<CancellationToken>()))
				.ReturnsAsync((new[] { b3, b4 }, 5));

			var result = await _handler.Handle(query, CancellationToken.None);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			Assert.Equal(5, result.Value!.TotalCount);
			Assert.Equal(3, result.Value.TotalPages);
			Assert.Equal(2, result.Value.Items.Count);

			var first = result.Value.Items[0];
			var second = result.Value.Items[1];

			Assert.Equal(b3.Id, first.Id);
			Assert.Equal(b3.ClientId, first.ClientId);
			Assert.Equal(b3.CityId, first.CityId);
			Assert.Equal("C", first.Street);
			Assert.Equal("3", first.Number);
			Assert.Equal(b3.ConstructionYear, first.ConstructionYear);
			Assert.Equal(b3.Type, first.Type);
			Assert.Equal(b3.NumberOfFloors, first.NumberOfFloors);
			Assert.Equal(b3.SurfaceArea, first.SurfaceArea);
			Assert.Equal(b3.InsuredValue, first.InsuredValue);

			Assert.Equal(b4.Id, second.Id);
			Assert.Equal("D", second.Street);
			Assert.Equal("4", second.Number);

			_buildingRepositoryMock.Verify(rp => rp.GetByClientIdPagedAsync(clientId, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
		}
	}
}