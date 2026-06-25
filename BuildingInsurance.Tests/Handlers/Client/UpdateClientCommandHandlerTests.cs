using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient;

namespace BuildingInsurance.Tests.Handlers.Client
{
    public class UpdateClientCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ILogger<UpdateClientCommandHandler>> _loggerMock;

        private readonly UpdateClientCommandHandler _handler;

        public UpdateClientCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _loggerMock = new Mock<ILogger<UpdateClientCommandHandler>>();

            _unitOfWorkMock.SetupGet(u => u.Clients).Returns(_clientRepositoryMock.Object);

            _handler = new UpdateClientCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnNotFound_WhenClientDoesNotExist()
        {
            var command = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "New Name",
                Email = "new@mail.com",
                Phone = "0700000000",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entities.Clients.Client?)null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Client not found.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsForOtherClientAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenIdentificationNumberExistsForOtherClient()
        {
            var existingClient = new Domain.Entities.Clients.Client(
                type: ClientType.Individual,
                fullName: "Old Name",
                contactInfo: new ContactInfo("old@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            var command = new UpdateClientCommand
            {
                ClientId = existingClient.Id,
                FullName = "New Name",
                Email = "new@mail.com",
                Phone = "0700000000",
                Address = null,
                IdentificationNumber = "  1234567890123  ",
                IdentificationChangeReason = "Correction"
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            _clientRepositoryMock
                .Setup(r => r.IdentificationNumberExistsForOtherClientAsync(existingClient.Id, "1234567890123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("Identification number already exists.", result.Error);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsForOtherClientAsync(
                existingClient.Id, "1234567890123", It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleShouldReturnSuccess_ValidInput_WithoutIdentifierChange()
        {
            var existingClient = new Domain.Entities.Clients.Client(
                type: ClientType.Individual,
                fullName: "Old Name",
                contactInfo: new ContactInfo("old@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            var command = new UpdateClientCommand
            {
                ClientId = existingClient.Id,
                FullName = "Updated Name",
                Email = "updated@mail.com",
                Phone = "0799999999",
                Address = new()
                {
                    Street = "Updated St",
                    Number = "22"
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(existingClient.Id, result.Value!.Id);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsForOtherClientAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleShouldReturnSuccess_ValidInput_WithIdentifierChange()
        {
            var existingClient = new Domain.Entities.Clients.Client(
                type: ClientType.Individual,
                fullName: "Old Name",
                contactInfo: new ContactInfo("old@mail.com", "0700000000", null),
                personalIdentificationNumber: "1111111111111",
                companyRegistrationNumber: null
            );

            var command = new UpdateClientCommand
            {
                ClientId = existingClient.Id,
                FullName = "Updated Name",
                Email = "updated@mail.com",
                Phone = "0799999999",
                Address = null,
                IdentificationNumber = "  9999999999999  ",
                IdentificationChangeReason = "Typo correction"
            };

            _clientRepositoryMock
                .Setup(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingClient);

            _clientRepositoryMock
                .Setup(r => r.IdentificationNumberExistsForOtherClientAsync(existingClient.Id, "9999999999999", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.Equal(existingClient.Id, result.Value!.Id);

            _clientRepositoryMock.Verify(r => r.GetByIdAsync(command.ClientId, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsForOtherClientAsync(
                existingClient.Id, "9999999999999", It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}