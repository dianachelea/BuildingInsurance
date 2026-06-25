using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCountries;

namespace BuildingInsurance.Tests.Validators.Geography
{
    public class ListCountriesQueryValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var validator = new ListCountriesQueryValidator();
            var query = new ListCountriesQuery{ Page = 1, PageSize = 20 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Page_Is_Invalid(int page)
        {
            var validator = new ListCountriesQueryValidator();
            var query = new ListCountriesQuery{ Page = page, PageSize = 20 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Out_Of_Range(int pageSize)
        {
            var validator = new ListCountriesQueryValidator();
            var query = new ListCountriesQuery { Page = 1, PageSize = pageSize };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}