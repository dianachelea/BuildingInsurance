using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCounties;

namespace BuildingInsurance.Tests.Validators.Geography
{
    public class ListCountiesByCountryQueryValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var validator = new ListCountiesByCountryQueryValidator();
            var query = new ListCountiesByCountryQuery{ CountryId = Guid.NewGuid(), Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_CountryId_Is_Empty()
        {
            var validator = new ListCountiesByCountryQueryValidator();
            var query = new ListCountiesByCountryQuery{ CountryId = Guid.Empty, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CountryId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Page_Is_Invalid(int page)
        {
            var validator = new ListCountiesByCountryQueryValidator();
            var query = new ListCountiesByCountryQuery{ CountryId = Guid.NewGuid(), Page = page, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Out_Of_Range(int pageSize)
        {
            var validator = new ListCountiesByCountryQueryValidator();
            var query = new ListCountiesByCountryQuery{ CountryId = Guid.NewGuid(), Page = 1, PageSize = pageSize };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}