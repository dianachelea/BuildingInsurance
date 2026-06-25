using BuildingInsurance.Application.Features.Brokers.Clients.Commands.UpdateClient;

namespace BuildingInsurance.Tests.Validators.Client
{
    public class UpdateClientCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Input()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.Empty,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ClientId" && e.ErrorMessage == "Client ID is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "   ",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Full name is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = new string('a', 201),
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Full name must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "   ",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Should_Fail_When_Email_Format_Is_Invalid()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "not-an-email",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email format is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Email_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = new string('a', 201) + "@x.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "   ",
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone number is required.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = new string('1', 21),
                Address = null,
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "The length of 'Phone' must be 20 characters or fewer. You entered 21 characters.");
        }

        [Fact]
        public void Address_Should_Fail_When_Street_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = new()
                {
                    Street = "   ",
                    Number = "10"
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Street is required.");
        }

        [Fact]
        public void Address_Should_Fail_When_Number_Is_Empty()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = new()
                {
                    Street = "Main St",
                    Number = "   "
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Number" && e.ErrorMessage == "Address number is required.");
        }

        [Fact]
        public void Address_Should_Fail_When_Street_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = new()
                {
                    Street = new string('a', 201),
                    Number = "10"
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Street must not exceed 200 characters.");
        }

        [Fact]
        public void Address_Should_Fail_When_Number_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = new()
                {
                    Street = "Main St",
                    Number = new string('1', 21)
                },
                IdentificationNumber = null,
                IdentificationChangeReason = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Number" && e.ErrorMessage == "Address number must not exceed 20 characters.");
        }

        [Fact]
        public void IdentificationChange_Should_Fail_When_Reason_Missing()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = "1234567890",
                IdentificationChangeReason = "   "
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "IdentificationChangeReason" && e.ErrorMessage == "Reason is required when changing identification number.");
        }

        [Fact]
        public void IdentificationChange_Should_Fail_When_IdentificationNumber_Too_Long()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = new string('1', 21),
                IdentificationChangeReason = "Typo fix"
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "IdentificationNumber" && e.ErrorMessage == "Identification number must not exceed 20 characters.");
        }

        [Fact]
        public void IdentificationChange_Should_Pass_When_Number_And_Reason_Are_Provided()
        {
            var validator = new UpdateClientCommandValidator();
            var cmd = new UpdateClientCommand
            {
                ClientId = Guid.NewGuid(),
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                Address = null,
                IdentificationNumber = "1234567890",
                IdentificationChangeReason = "Typo fix"
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }
    }
}