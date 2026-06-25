using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Commands.UpdateCurrency;

namespace BuildingInsurance.Tests.Validators.Currency
{
    public class UpdateCurrencyCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "Euro",
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Empty()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.Empty,
                Name = "Euro",
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage == "Currency id is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Null()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = null!,
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Currency name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "",
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Currency name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Whitespace()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "   ",
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Currency name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Too_Long()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = new string('a', 101),
                ExchangeRateToBase = 4.95m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Currency name must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Fail_When_ExchangeRateToBase_Is_Zero()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "Euro",
                ExchangeRateToBase = 0m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ExchangeRateToBase" && e.ErrorMessage == "ExchangeRateToBase must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_ExchangeRateToBase_Is_Negative()
        {
            var validator = new UpdateCurrencyCommandValidator();
            var cmd = new UpdateCurrencyCommand
            {
                Id = Guid.NewGuid(),
                Name = "Euro",
                ExchangeRateToBase = -10m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ExchangeRateToBase" && e.ErrorMessage == "ExchangeRateToBase must be greater than 0.");
        }
    }
}