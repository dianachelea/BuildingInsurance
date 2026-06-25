using BuildingInsurance.Application.Features.Brokers.Clients.Commands.CreateClient;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.Client
{
    public class CreateClientCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Individual()
        {
            var validator = new CreateClientCommandValidator();
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

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_For_Valid_Company()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Company,
                FullName = "ACME SRL",
                Email = "office@acme.ro",
                Phone = "0711111111",
                PersonalIdentificationNumber = null,
                CompanyRegistrationNumber = "RO12345678",
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Type_Is_Invalid()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = (ClientTypeContract)999,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Type" && e.ErrorMessage == "Client type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Is_Empty()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "   ",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Full name is required.");
        }

        [Fact]
        public void Should_Fail_When_FullName_Too_Long()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = new string('a', 201),
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FullName" && e.ErrorMessage == "Full name must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Email_Is_Empty()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "   ",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email is required.");
        }

        [Fact]
        public void Should_Fail_When_Email_Format_Is_Invalid()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "not-an-email",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email format is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Email_Too_Long()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = new string('a', 201) + "@x.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorMessage == "Email must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Is_Empty()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "   ",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "Phone number is required.");
        }

        [Fact]
        public void Should_Fail_When_Phone_Too_Long()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = new string('1', 21),
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Phone" && e.ErrorMessage == "The length of 'Phone' must be 20 characters or fewer. You entered 21 characters.");
        }

        [Fact]
        public void Individual_Should_Fail_When_CNP_Missing()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "   ",
                CompanyRegistrationNumber = null,
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PersonalIdentificationNumber" && e.ErrorMessage == "Personal identification number is required for individual clients.");
        }

        [Fact]
        public void Individual_Should_Fail_When_CompanyRegistrationNumber_Is_Set()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = "RO123",
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CompanyRegistrationNumber" && e.ErrorMessage == "Company registration number must be empty for individual clients.");
        }

        [Fact]
        public void Company_Should_Fail_When_CUI_Missing()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Company,
                FullName = "ACME SRL",
                Email = "office@acme.ro",
                Phone = "0711111111",
                PersonalIdentificationNumber = null,
                CompanyRegistrationNumber = "   ",
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CompanyRegistrationNumber" && e.ErrorMessage == "Company registration number is required for company clients.");
        }

        [Fact]
        public void Company_Should_Fail_When_PersonalIdentificationNumber_Is_Set()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Company,
                FullName = "ACME SRL",
                Email = "office@acme.ro",
                Phone = "0711111111",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = "RO12345678",
                Address = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PersonalIdentificationNumber" && e.ErrorMessage == "Personal identification number must be empty for company clients.");
        }

        [Fact]
        public void Address_Should_Fail_When_Street_Is_Empty()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = new()
                {
                    Street = "   ",
                    Number = "10"
                }
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Address street is required.");
        }

        [Fact]
        public void Address_Should_Fail_When_Number_Is_Empty()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = new()
                {
                    Street = "Main St",
                    Number = "   "
                }
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Number" && e.ErrorMessage == "Address number is required.");
        }

        [Fact]
        public void Address_Should_Fail_When_Street_Too_Long()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = new()
                {
                    Street = new string('a', 201),
                    Number = "10"
                }
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Address street must not exceed 200 characters.");
        }

        [Fact]
        public void Address_Should_Fail_When_Number_Too_Long()
        {
            var validator = new CreateClientCommandValidator();
            var cmd = new CreateClientCommand
            {
                Type = ClientTypeContract.Individual,
                FullName = "John Doe",
                Email = "john@example.com",
                Phone = "0712345678",
                PersonalIdentificationNumber = "1234567890123",
                CompanyRegistrationNumber = null,
                Address = new()
                {
                    Street = "Main St",
                    Number = new string('1', 21)
                }
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Number" && e.ErrorMessage == "Address number must not exceed 20 characters.");
        }
    }
}