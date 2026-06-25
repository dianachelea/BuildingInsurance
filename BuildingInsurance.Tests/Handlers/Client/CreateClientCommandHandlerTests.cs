using BuildingInsurance.Application.Features.Common.Result;
using BuildingInsurance.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;
using Moq;
using BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient;
using BuildingInsurance.Application.Features.Common.Mapping;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Handlers.Clients
{
    public class CreateClientCommandHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<ILogger<CreateClientCommandHandler>> _loggerMock;

        private readonly CreateClientCommandHandler _handler;

        public CreateClientCommandHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _loggerMock = new Mock<ILogger<CreateClientCommandHandler>>();
            _unitOfWorkMock.SetupGet(u => u.Clients).Returns(_clientRepositoryMock.Object);
            _handler = new CreateClientCommandHandler(_unitOfWorkMock.Object, _loggerMock.Object);
        }
    
        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenEmailAlreadyExists()
        {
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            _clientRepositoryMock
                .Setup(x => x.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("A client with this email already exists.", result.Error);

            _clientRepositoryMock.Verify(x => x.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(x => x.IdentificationNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Clients.Client>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenIdentificationNumberAlreadyExists_ForIndividual()
        {
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            _clientRepositoryMock
                .Setup(r => r.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _clientRepositoryMock
                .Setup(r => r.IdentificationNumberExistsAsync(cmd.PersonalIdentificationNumber!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("A client with this identification number already exists.", result.Error);

            _clientRepositoryMock.Verify(r => r.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsAsync(cmd.PersonalIdentificationNumber!, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Domain.Entities.Clients.Client>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnConflict_WhenIdentificationNumberAlreadyExists_ForCompany()
        {
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Company,
                FullName = "ACME SRL",
                Email = "office@acme.ro",
                Phone = "0711111111",
                PersonalIdentificationNumber = null,
                CompanyRegistrationNumber = "J40/1234/2020",
                Address = null
            };

            _clientRepositoryMock
                .Setup(r => r.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _clientRepositoryMock
                .Setup(r => r.IdentificationNumberExistsAsync(cmd.CompanyRegistrationNumber!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal("A client with this identification number already exists.", result.Error);

            _clientRepositoryMock.Verify(r => r.EmailExistsAsync(cmd.Email, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsAsync(cmd.CompanyRegistrationNumber!, It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BuildingInsurance.Domain.Entities.Clients.Client>(), It.IsAny<CancellationToken>()), Times.Never);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleShouldReturnSuccess_ValidInput()
        {
            var command = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            _clientRepositoryMock
                .Setup(r => r.EmailExistsAsync(command.Email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _clientRepositoryMock
                .Setup(r => r.IdentificationNumberExistsAsync(command.PersonalIdentificationNumber!, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.NotNull(result.Value);
            Assert.NotEqual(Guid.Empty, result.Value!.Id);
            Assert.Equal(command.Email, result.Value.Email);
            Assert.Equal(command.FullName, result.Value.FullName);

            _clientRepositoryMock.Verify(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _clientRepositoryMock.Verify(r => r.IdentificationNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            _clientRepositoryMock.Verify(r => r.AddAsync(It.Is<Domain.Entities.Clients.Client>(c =>
                c.Type == command.Type.MapToDomainClientType() &&
                c.FullName == command.FullName &&
                c.ContactInfo.Email == command.Email &&
                c.ContactInfo.Phone == command.Phone
            ), It.IsAny<CancellationToken>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}