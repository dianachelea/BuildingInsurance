using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.CreateBroker;

namespace BuildingInsurance.Tests.Validators.Broker
{
    public class CreateBrokerCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_CommissionPercentage_Is_Null()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_BrokerCode_Is_Empty()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "   ",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerCode" && e.ErrorMessage == "Broker code is required.");
        }

        [Fact]
        public void Should_Fail_When_BrokerCode_Too_Short()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "A",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerCode" && e.ErrorMessage == "The length of 'Broker Code' must be at least 2 characters. You entered 1 characters.");
        }

        [Fact]
        public void Should_Fail_When_BrokerCode_Too_Long()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = new string('a', 31),
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerCode" && e.ErrorMessage == "Broker code must be between 2 and 30 characters.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Is_Empty()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "   ",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Broker name is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Short()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "AB",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "The length of 'Full Name' must be at least 3 characters. You entered 2 characters.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Long()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = new string('a', 201),
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Broker name must be between 3 and 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Empty()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "   ",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Invalid()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "not-an-email",
                Phone = "+40712345678",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Is_Empty()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "   ",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone is required.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Has_Invalid_Characters()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40 712 345",
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone must contain only digits and optional leading '+'.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Too_Long()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = new string('1', 31),
                CommissionPercentage = 0.25m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone must not exceed 30 characters.");
        }

        [Fact]
        public void Should_Fail_When_CommissionPercentage_Is_Zero()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 0m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CommissionPercentage" && e.ErrorMessage == "'Commission Percentage' must be greater than '0'.");
        }

        [Fact]
        public void Should_Fail_When_CommissionPercentage_Is_One()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = 1m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CommissionPercentage" && e.ErrorMessage == "CommissionPercentage must be between 0 (exclusive) and 1 (exclusive).");
        }

        [Fact]
        public void Should_Fail_When_CommissionPercentage_Is_Negative()
        {
            var validator = new CreateBrokerCommandValidator();
            var cmd = new CreateBrokerCommand
            {
                BrokerCode = "BR01",
                FullName = "John Broker",
                Email = "john.broker@test.com",
                Phone = "+40712345678",
                CommissionPercentage = -0.1m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CommissionPercentage" && e.ErrorMessage == "'Commission Percentage' must be greater than '0'.");
        }
    }
}