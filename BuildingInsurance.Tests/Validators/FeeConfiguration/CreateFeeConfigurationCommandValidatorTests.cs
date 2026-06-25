using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.CreateFeeConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.FeeConfiguration
{
    public sealed class CreateFeeConfigurationCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command_StandardFee()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Admin Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_For_Valid_Command_RiskAdjustment()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Flood Risk Fee",
                FeeType = FeeTypeContract.RiskAdjustment,
                FeePercentage = 0.15m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Null()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = null!,
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Whitespace()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "   ",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Too_Long()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = new string('a', 101),
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name must be between 3 and 100 characters.");
        }

        [Fact]
        public void Should_Fail_When_FeeType_Is_Invalid()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Fee",
                FeeType = (FeeTypeContract)999,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeeType" && e.ErrorMessage == "Fee type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_Negative()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = -0.01m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && e.ErrorMessage.Contains("'Fee Percentage' must be greater than '0'."));
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_Greater_Than_1()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 1.01m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && e.ErrorMessage == "Fee percentage must be between 0 and 1.");
        }

        [Fact]
        public void Should_Fail_When_EffectiveTo_Is_Before_EffectiveFrom()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(-1),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "EffectiveTo" && e.ErrorMessage == "EffectiveTo must be after EffectiveFrom.");
        }

        [Fact]
        public void Should_Fail_When_RiskAdjustment_And_RiskIndicators_None()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Risk Fee",
                FeeType = FeeTypeContract.RiskAdjustment,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "RiskAdjustment fees must specify at least one risk indicator.");
        }

        [Fact]
        public void Should_Fail_When_Not_RiskAdjustment_But_RiskIndicators_Specified()
        {
            var validator = new CreateFeeConfigurationCommandValidator();
            var cmd = new CreateFeeConfigurationCommand
            {
                Name = "Standard Fee",
                FeeType = FeeTypeContract.BrokerCommission,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.Date,
                EffectiveTo = DateTime.UtcNow.Date.AddDays(10),
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "Only RiskAdjustment fees can have risk indicators.");
        }
    }
}