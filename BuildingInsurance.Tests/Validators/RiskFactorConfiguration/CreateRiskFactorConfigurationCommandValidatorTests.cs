using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.CreateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.RiskFactorConfiguration
{
    public class CreateRiskFactorConfigurationCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_BuildingType_Level()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.15m,
                BuildingType = BuildingTypeContract.Residential,
                ReferenceId = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Invalid()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = (RiskFactorLevelContract)999,
                AdjustmentPercentage = 0.1m
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Level" && e.ErrorMessage == "Risk factor level is invalid.");
        }

        [Fact]
        public void Should_Pass_When_AdjustmentPercentage_Is_Negative()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.Country,
                AdjustmentPercentage = -0.01m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_AdjustmentPercentage_Greater_Than_One()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.Country,
                AdjustmentPercentage = 1.5m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdjustmentPercentage" && e.ErrorMessage.Contains("must be less than '1'"));
        }

        [Fact]
        public void Should_Fail_When_Level_Is_BuildingType_And_BuildingType_Is_Null()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.2m,
                BuildingType = null,
                ReferenceId = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BuildingType" && e.ErrorMessage == "BuildingType is required when Level is BuildingType.");
        }

        [Fact]
        public void Should_Fail_When_Level_Is_BuildingType_And_ReferenceId_Is_Not_Null()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.2m,
                BuildingType = BuildingTypeContract.Industrial,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId must be null when Level is BuildingType.");
        }

        [Fact]
        public void Should_Pass_For_Valid_Geographic_Level()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0.05m,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Geographic_Level_And_ReferenceId_Is_Null()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.County,
                AdjustmentPercentage = 0.05m,
                ReferenceId = null,
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId is required for geographic levels.");
        }

        [Fact]
        public void Should_Fail_When_Geographic_Level_And_ReferenceId_Is_Empty_Guid()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.Country,
                AdjustmentPercentage = 0.05m,
                ReferenceId = Guid.Empty,
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId must be a non-empty GUID.");
        }

        [Fact]
        public void Should_Fail_When_Geographic_Level_And_BuildingType_Is_Not_Null()
        {
            var validator = new CreateRiskFactorConfigurationCommandValidator();
            var cmd = new CreateRiskFactorConfigurationCommand
            {
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0.05m,
                ReferenceId = Guid.NewGuid(),
                BuildingType = BuildingTypeContract.Residential
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BuildingType" && e.ErrorMessage == "BuildingType must be null for geographic levels.");
        }
    }
}