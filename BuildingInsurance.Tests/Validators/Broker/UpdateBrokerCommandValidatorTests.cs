using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.UpdateBroker;

namespace BuildingInsurance.Tests.Validators.Broker
{
    public class UpdateBrokerCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_CommissionPercentage_Is_Null()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Empty()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.Empty,
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage == "Broker id is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Is_Empty()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "   ",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Broker name is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Short()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "AB",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "The length of 'Full Name' must be at least 3 characters. You entered 2 characters.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Long()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = new string('a', 201),
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Broker name must be between 3 and 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Empty()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "   ",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Invalid()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "not-an-email",
                Phone = "0712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Is_Empty()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "   ",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone is required.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Contains_Non_Digits()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone must contain only numeric characters.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Too_Long()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = new string('1', 21),
                CommissionPercentage = 0.3m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone must not exceed 20 characters.");
        }

        [Fact]
        public void Should_Fail_When_CommissionPercentage_Is_Zero()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 0m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CommissionPercentage" && e.ErrorMessage == "'Commission Percentage' must be greater than '0'.");
        }

        [Fact]
        public void Should_Fail_When_CommissionPercentage_Is_One()
        {
            var validator = new UpdateBrokerCommandValidator();
            var cmd = new UpdateBrokerCommand
            {
                Id = Guid.NewGuid(),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "0712345678",
                CommissionPercentage = 1m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CommissionPercentage" && e.ErrorMessage == "CommissionPercentage must be between 0 (exclusive) and 1 (exclusive).");
        }
    }
}