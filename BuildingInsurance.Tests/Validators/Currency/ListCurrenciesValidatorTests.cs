using BuildingInsurance.Application.Features.Administrators.Metadata.Currency.Queries.ListCurrencies;

namespace BuildingInsurance.Tests.Validators.Currency
{
    public class ListCurrenciesValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query()
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery
            {
                Name = "Euro",
                IsActive = true,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Null_Because_Rule_Is_Conditional()
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery {
                Name = null,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Whitespace_Because_Rule_Is_Conditional()
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery {
                Name = "   ",
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Name_Too_Long()
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery {
                Name = new string('a', 101),
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Currency name must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Less_Than_1()
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery {
                Name = "Euro",
                IsActive = null,
                Page = 0,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Is_Out_Of_Range(int pageSize)
        {
            var validator = new ListCurrenciesValidator();
            var query = new ListCurrenciesQuery {
                Name = "Euro",
                IsActive = null,
                Page = 1,
                PageSize = pageSize
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}