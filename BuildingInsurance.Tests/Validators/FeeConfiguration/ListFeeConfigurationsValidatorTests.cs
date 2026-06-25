using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Queries.ListFeeConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.FeeConfiguration
{
    public sealed class ListFeeConfigurationsValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery
            {
                Name = "Processing Fee",
                Type = FeeTypeContract.BrokerCommission,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Null()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery {
                Name = null,
                Type = FeeTypeContract.BrokerCommission,
                IsActive = null,
                Page = 1,
                PageSize = 20};

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Name_Is_Whitespace()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery
            {
                Name = "   ",
                Type = FeeTypeContract.BrokerCommission,
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
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery
            {
                Name = new string('a', 101),
                Type = FeeTypeContract.AdminFee,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name must not exceed 100 characters.");
        }

        [Fact]
        public void Should_Fail_When_Type_Is_Invalid()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery
            {
                Name = "Fee",
                Type = (FeeTypeContract)999,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e =>
                e.PropertyName == "Type" &&
                e.ErrorMessage == "Fee type is invalid.");
        }

        [Fact]
        public void Should_Pass_When_Type_Is_Null()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery {
                Name = "Fee",
                Type = null,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Zero()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery {
                Name = "Fee",
                Type = FeeTypeContract.AdminFee,
                IsActive = null,
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
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery {
                Name = "Fee",
                Type = FeeTypeContract.AdminFee,
                IsActive = null,
                Page = -1,
                PageSize = 20};

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Page");
        }

        [Fact]
        public void Should_Fail_When_PageSize_Is_Zero()
        {
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery
            {
                Name = "Fee",
                Type = FeeTypeContract.BrokerCommission,
                IsActive = null,
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
            var validator = new ListFeeConfigurationsValidator();
            var query = new ListFeeConfigurationsQuery {
                Name = "Fee",
                Type = FeeTypeContract.RiskAdjustment,
                IsActive = null,
                Page = 1,
                PageSize = 101};

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}