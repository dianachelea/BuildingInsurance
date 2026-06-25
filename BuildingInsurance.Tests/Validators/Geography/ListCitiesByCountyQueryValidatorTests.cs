using BuildingInsurance.Application.Features.Brokers.Geography.Queries.ListCities;

namespace BuildingInsurance.Tests.Validators.Geography
{
    public class ListCitiesByCountyQueryValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var validator = new ListCitiesByCountyQueryValidator();
            var query = new ListCitiesByCountyQuery{ CountyId = Guid.NewGuid(), Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_CountyId_Is_Empty()
        {
            var validator = new ListCitiesByCountyQueryValidator();
            var query = new ListCitiesByCountyQuery{ CountyId = Guid.Empty, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CountyId");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Page_Is_Invalid(int page)
        {
            var validator = new ListCitiesByCountyQueryValidator();
            var query = new ListCitiesByCountyQuery{ CountyId = Guid.NewGuid(), Page = page, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Out_Of_Range(int pageSize)
        {
            var validator = new ListCitiesByCountyQueryValidator();
            var query = new ListCitiesByCountyQuery{ CountyId = Guid.NewGuid(), Page = 1, PageSize = pageSize };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}