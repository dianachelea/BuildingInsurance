using BuildingInsurance.Application.Features.Administrators.Brokers.Queries.ListBrokers;

namespace BuildingInsurance.Tests.Validators.Broker
{
    public class ListBrokersValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery
            {
                Name = "John",
                IsActive = null,
                Page = 1,
                PageSize = 10
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Null()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery
            {
                Name = null,
                IsActive = null,
                Page = 1,
                PageSize = 10
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Empty()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "   ", IsActive = null, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Name_Too_Long()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = new string('a', 201), IsActive = null, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Name must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Zero()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = 0, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Negative()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = -1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Zero()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = 1, PageSize = 0 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Greater_Than_100()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = 1, PageSize = 101 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_1()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = 1, PageSize = 1 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_100()
        {
            var validator = new ListBrokersValidator();
            var query = new ListBrokersQuery{ Name = "John", IsActive = null, Page = 1, PageSize = 15 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }
    }
}