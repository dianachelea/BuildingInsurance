using BuildingInsurance.Application.Features.Administrators.Metadata.FeeConfiguration.Commands.UpdateFeeConfiguration;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.FeeConfiguration
{
    public sealed class UpdateFeeConfigurationCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command_When_Not_RiskAdjustment()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Pass_For_Valid_Command_When_RiskAdjustment_With_RiskIndicators()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Risk Adj Fee",
                FeePercentage = 0.05m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.RiskAdjustment,
                RiskIndicators = RiskIndicatorsContract.EarthquakeProne
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_Id_Is_Empty()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.Empty,
                Name = "Processing Fee",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage == "Fee configuration id is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Null()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = null!,
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Empty()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Whitespace()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "   ",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name is required.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Too_Short()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Ab",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "The length of 'Name' must be at least 3 characters. You entered 2 characters.");
        }

        [Fact]
        public void Should_Fail_When_Name_Is_Too_Long()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = new string('a', 101),
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Fee name must be between 3 and 100 characters.");
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_Zero()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 0m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && (e.ErrorMessage == "Fee percentage must be between 0 and 1." || e.ErrorMessage.Contains("must be greater than")));
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_Negative()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = -0.01m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && (e.ErrorMessage == "Fee percentage must be between 0 and 1." || e.ErrorMessage.Contains("must be greater than")));
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_One()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 1m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && (e.ErrorMessage == "Fee percentage must be between 0 and 1." || e.ErrorMessage.Contains("must be less than")));
        }

        [Fact]
        public void Should_Fail_When_FeePercentage_Is_Greater_Than_One()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 1.01m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeePercentage" && (e.ErrorMessage == "Fee percentage must be between 0 and 1." || e.ErrorMessage.Contains("must be less than")));
        }

        [Fact]
        public void Should_Fail_When_EffectiveTo_Is_Before_EffectiveFrom()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(5),
                EffectiveTo = DateTime.UtcNow.AddDays(1),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "EffectiveTo" && e.ErrorMessage == "EffectiveTo must be after EffectiveFrom.");
        }

        [Fact]
        public void Should_Fail_When_FeeType_Is_Invalid()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = (FeeTypeContract)999,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "FeeType" && e.ErrorMessage == "Fee type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_FeeType_Is_RiskAdjustment_And_RiskIndicators_Is_None()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Risk Adj Fee",
                FeePercentage = 0.05m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.RiskAdjustment,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "RiskAdjustment fee must specify risk indicators.");
        }

        [Fact]
        public void Should_Fail_When_FeeType_Is_Not_RiskAdjustment_And_RiskIndicators_Is_Not_None()
        {
            var validator = new UpdateFeeConfigurationCommandValidator();
            var cmd = new UpdateFeeConfigurationCommand
            {
                Id = Guid.NewGuid(),
                Name = "Processing Fee",
                FeePercentage = 0.10m,
                EffectiveFrom = DateTime.UtcNow.AddDays(-1),
                EffectiveTo = DateTime.UtcNow.AddDays(10),
                FeeType = FeeTypeContract.AdminFee,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "Only RiskAdjustment fees can have risk indicators.");
        }
    }
}