using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Brokers.Clients.Queries.GetClient;
using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;
using Moq;
using Xunit;

namespace BuildingInsurance.Tests.Handlers.Client
{
    public class GetClientByIdHandlerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock; 
        private readonly GetClientByIdHandler _handler;

        public GetClientByIdHandlerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _handler = new GetClientByIdHandler(_clientRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenClientDoesNotExist()
        {
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Clients.Client?)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal($"Client with id {clientId} not found.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenClientExists_WithoutAddress()
        {
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            var contactInfo = new ContactInfo(
                email: "john@example.com",
                phone: "0712345678",
                address: null);

            var client = new Domain.Entities.Clients.Client(
                id: clientId,
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: contactInfo,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);

            Assert.Equal(client.Id, result.Value!.Id);
            Assert.Equal(ClientType.Individual, result.Value.Type);
            Assert.Equal("John Doe", result.Value.FullName);
            Assert.Equal("1234567890123", result.Value.PersonalIdentificationNumber);
            Assert.Null(result.Value.CompanyRegistrationNumber);
            Assert.Equal("john@example.com", result.Value.Email);
            Assert.Equal("0712345678", result.Value.Phone);
            Assert.Null(result.Value.Address);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenClientExists_WithAddress()
        {
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            var address = new Address(
                street: "Main St",
                number: "10");

            var contactInfo = new ContactInfo(
                email: "john@example.com",
                phone: "0712345678",
                address: address);

            var client = new Domain.Entities.Clients.Client(
                id: clientId,
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: contactInfo,
                personalIdentificationNumber: "1234567890123",
                companyRegistrationNumber: null);

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(client);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.NotNull(result.Value!.Address);

            Assert.Equal("MAIN ST", result.Value.Address!.Street);
            Assert.Equal("10", result.Value.Address.Number);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}