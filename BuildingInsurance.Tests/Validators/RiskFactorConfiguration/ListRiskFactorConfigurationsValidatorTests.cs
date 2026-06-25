using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Queries.ListRiskFactorConfigurations;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.RiskFactorConfiguration
{
    public sealed class ListRiskFactorConfigurationsValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Query_With_All_Fields()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                IsActive = null,
                Page = 1,
                PageSize = 20};

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_Level_Is_Null()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery {
                Level = null,
                ReferenceId = Guid.NewGuid(),
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_When_ReferenceId_Is_Null()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.County,
                ReferenceId = null,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Invalid()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = (RiskFactorLevelContract)999,
                ReferenceId = Guid.NewGuid(),
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Level" && e.ErrorMessage == "Risk factor level is invalid.");
        }

        [Fact]
        public void Should_Fail_When_ReferenceId_Is_Empty()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.Country,
                ReferenceId = Guid.Empty,
                IsActive = null,
                Page = 1,
                PageSize = 20
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId must be a valid GUID.");
        }

        [Fact]
        public void Should_Fail_When_Page_Is_Zero()
        {
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
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
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                IsActive = null,
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
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
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
            var validator = new ListRiskFactorConfigurationsValidator();
            var query = new ListRiskFactorConfigurationsQuery
            {
                Level = RiskFactorLevelContract.City,
                ReferenceId = Guid.NewGuid(),
                IsActive = null,
                Page = 1,
                PageSize = 101
            };

            var result = validator.Validate(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PageSize");
        }
    }
}