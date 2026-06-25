using BuildingInsurance.Application.Features.Brokers.Buildings.Queries.ListBuildingsByClient;

namespace BuildingInsurance.Tests.Validators.Building
{
    public class ListBuildingsByClientValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query()
        {
            var validator = new ListBuildingsByClientValidator();
            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery { ClientId = Guid.Empty, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ClientId" && e.ErrorMessage == "ClientId is required.");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Zero()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 0, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Negative()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery { ClientId = Guid.NewGuid(), Page = -1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Zero()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 1, PageSize = 0 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Greater_Than_100()
        {
            var validator = new ListBuildingsByClientValidator();
            
            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 1, PageSize = 101 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_1()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 1, PageSize = 1 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_100()
        {
            var validator = new ListBuildingsByClientValidator();

            var query = new ListBuildingsByClientQuery{ ClientId = Guid.NewGuid(), Page = 1, PageSize = 100 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }
    }
}