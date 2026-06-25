using BuildingInsurance.Application.Features.Brokers.Policies.Queries.ListPolicies;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Domain.Enums;

namespace BuildingInsurance.Tests.Validators.Policy
{
    public sealed class ListPoliciesValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query_With_All_Fields()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_ClientId_Is_Null()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery {
                ClientId = null,
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_BrokerId_Is_Null()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = null,
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Status_Is_Null()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.Empty,
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ClientId" && e.ErrorMessage == "ClientId is invalid.");
        }

        [Fact]
        public void Should_Fail_When_BrokerId_Is_Empty()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.Empty,
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerId" && e.ErrorMessage == "BrokerId is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Status_Is_Invalid()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery 
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = (PolicyStatusContract)999,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage == "Policy status is invalid.");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Zero()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 0,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Negative()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = -1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Zero()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 0
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Greater_Than_100()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 101
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_1()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 1
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_PageSize_Is_100()
        {
            var validator = new ListPoliciesValidator();
            var query = new ListPoliciesQuery
            {
                ClientId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                Status = PolicyStatusContract.Active,
                Page = 1,
                PageSize = 100
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }
    }
}