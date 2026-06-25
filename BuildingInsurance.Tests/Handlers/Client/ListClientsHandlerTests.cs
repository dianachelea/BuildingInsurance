using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using Moq;
using BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients;

namespace BuildingInsurance.Tests.Handlers.Client
{
    public class ListClientsHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly ListClientsHandler _handler;

        public ListClientsHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _handler = new ListClientsHandler(_clientRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoClientsFound()
        {
            var query = new ListClientsQuery{ Name = null, Identifier = null, Page = 1, PageSize = 10 };

			_clientRepositoryMock
		        .Setup(r => r.SearchPagedAsync(query.Name, query.Identifier, 1, 10, It.IsAny<CancellationToken>()))
		        .ReturnsAsync((Array.Empty<Domain.Entities.Clients.Client>(), 0));

			var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Empty(result.Value!.Items);
            Assert.Equal(0, result.Value.TotalCount);
            Assert.Equal(0, result.Value.TotalPages);

			_clientRepositoryMock.Verify(r => r.SearchPagedAsync(query.Name, query.Identifier, 1, 10, It.IsAny<CancellationToken>()), Times.Once);
		}

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenPageIsGreaterThanTotalPages()
        {
			var query = new ListClientsQuery { Name = null, Identifier = null, Page = 3, PageSize = 2 };

			_clientRepositoryMock
				.Setup(r => r.SearchPagedAsync(query.Name, query.Identifier, 3, 2, It.IsAny<CancellationToken>()))
				.ReturnsAsync((Array.Empty<Domain.Entities.Clients.Client>(), 3));

			var result = await _handler.Handle(query, CancellationToken.None);

			Assert.True(result.IsSuccess);
			Assert.NotNull(result.Value);

			Assert.Empty(result.Value!.Items);
			Assert.Equal(3, result.Value.TotalCount);
			Assert.Equal(2, result.Value.TotalPages);

			_clientRepositoryMock.Verify(r => r.SearchPagedAsync(query.Name, query.Identifier, 3, 2, It.IsAny<CancellationToken>()), Times.Once);
		}

        [Fact]
        public async Task Handle_ShouldReturnPagedItems_Correctly_AndSortedByFullName()
        {
            var query = new ListClientsQuery{ Name = null, Identifier = null, Page = 2, PageSize = 2 };

            var contactInfoCharlie = new ContactInfo("charlie@x.com", "0700000000", address: null);
            var clientCharlie = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Charlie",
                contactInfo: contactInfoCharlie,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            var contactInfoAlice = new ContactInfo("alice@x.com", "0700000000", address: null);
            var clientAlice = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Alice",
                contactInfo: contactInfoAlice,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            var contactInfoBob = new ContactInfo("bob@x.com", "0700000000", address: null);
            var clientBob = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Bob",
                contactInfo: contactInfoBob,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            var contactInfoDavid = new ContactInfo("david@x.com", "0700000000", address: null);
            var clientDavid = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "David",
                contactInfo: contactInfoDavid,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            var contactInfoEve = new ContactInfo("eve@x.com", "0700000000", address: null);
            var clientEve = new Domain.Entities.Clients.Client(
                id: Guid.NewGuid(),
                type: ClientType.Individual,
                fullName: "Eve",
                contactInfo: contactInfoEve,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

			_clientRepositoryMock
		        .Setup(r => r.SearchPagedAsync(query.Name, query.Identifier, 2, 2, It.IsAny<CancellationToken>()))
		        .ReturnsAsync((new[] { clientCharlie, clientDavid }, 5));

			var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(5, result.Value!.TotalCount);
            Assert.Equal(3, result.Value.TotalPages);
            Assert.Equal(2, result.Value.Items.Count);

            Assert.Equal("Charlie", result.Value.Items[0].FullName);
            Assert.Equal("David", result.Value.Items[1].FullName);

			_clientRepositoryMock.Verify(r => r.SearchPagedAsync(query.Name, query.Identifier, 2, 2, It.IsAny<CancellationToken>()), Times.Once);
		}
    }
}