using BuildingInsurance.Application.Features.Brokers.Clients.Queries.ListClients;

namespace BuildingInsurance.Tests.Validators.Client
{
    public class ListClientsValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Request()
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery{ Name = "John", Identifier = "123", Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Fail_When_Page_Is_Invalid(int page)
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery{ Name = null, Identifier = null, Page = page, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Should_Fail_When_PageSize_Out_Of_Range(int pageSize)
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery { Name = null, Identifier = null, Page = 1, PageSize = pageSize };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Fail_When_Name_Too_Long()
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery{ Name = new string('a', 201), Identifier = null, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        }

        [Fact]
        public void Should_Fail_When_Identifier_Too_Long()
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery{ Name = null, Identifier = new string('a', 51), Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Identifier");
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Whitespace()
        {
            var validator = new ListClientsValidator();
            var query = new ListClientsQuery{ Name = "   ", Identifier = null, Page = 1, PageSize = 10 };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }
    }
}