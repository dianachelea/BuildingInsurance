using BuildingInsurance.Application.Features.Administrators.Metadata.RiskFactorConfiguration.Commands.UpdateRiskFactorConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.RiskFactorConfiguration
{
    public sealed class UpdateRiskFactorConfigurationCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command_When_Level_Is_BuildingType()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.25m,
                BuildingType = BuildingTypeContract.Industrial,
                ReferenceId = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_For_Valid_Command_When_Level_Is_Geographic()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0.10m,
                ReferenceId = Guid.NewGuid(),
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Empty()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.Empty,
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0.10m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage == "Id is required.");
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Invalid()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = (RiskFactorLevelContract)999,
                AdjustmentPercentage = 0.10m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Level" && e.ErrorMessage == "Risk factor level is invalid.");
        }

        [Fact]
        public void Should_Fail_When_AdjustmentPercentage_Is_Zero()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdjustmentPercentage" && e.ErrorMessage == "RiskFactorConfiguration percentage must be between -1 and 1 (exclusive) and cannot be 0.");
        }

        [Fact]
        public void Should_Pass_When_AdjustmentPercentage_Is_Negative()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = -0.01m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_AdjustmentPercentage_Is_One()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 1m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdjustmentPercentage" && e.ErrorMessage.Contains("must be less than '1'"));
        }

        [Fact]
        public void Should_Fail_When_AdjustmentPercentage_Is_Greater_Than_One()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 1.01m,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "AdjustmentPercentage" && e.ErrorMessage.Contains("must be less than '1'"));
        }

        [Fact]
        public void Should_Fail_When_Level_Is_BuildingType_And_BuildingType_Is_Null()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.20m,
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
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.BuildingType,
                AdjustmentPercentage = 0.20m,
                BuildingType = BuildingTypeContract.Industrial,
                ReferenceId = Guid.NewGuid()
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId must be null when Level is BuildingType.");
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Geographic_And_ReferenceId_Is_Null()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.County,
                AdjustmentPercentage = 0.20m,
                ReferenceId = null,
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId is required for geographic levels.");
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Geographic_And_ReferenceId_Is_Empty_Guid()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.Country,
                AdjustmentPercentage = 0.20m,
                ReferenceId = Guid.Empty,
                BuildingType = null
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ReferenceId" && e.ErrorMessage == "ReferenceId must be a non-empty GUID.");
        }

        [Fact]
        public void Should_Fail_When_Level_Is_Geographic_And_BuildingType_Is_Not_Null()
        {
            var validator = new UpdateRiskFactorConfigurationCommandValidator();
            var cmd = new UpdateRiskFactorConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Level = RiskFactorLevelContract.City,
                AdjustmentPercentage = 0.20m,
                ReferenceId = Guid.NewGuid(),
                BuildingType = BuildingTypeContract.Industrial
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BuildingType" && e.ErrorMessage == "BuildingType must be null for geographic levels.");
        }
    }
}