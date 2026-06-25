using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CancelPolicy;

namespace BuildingInsurance.Tests.Validators.Policy
{
    public sealed class CancelPolicyCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "Non-payment",
                CancellationEffectiveDate = DateTime.UtcNow.AddDays(1)
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_PolicyId_Is_Empty()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.Empty,
                Reason = "Duplicate policy",
                CancellationEffectiveDate = DateTime.UtcNow.AddDays(1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PolicyId" && e.ErrorMessage == "PolicyId is required.");
        }

        [Fact]
        public void Should_Fail_When_Reason_Is_Null()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = null!,
                CancellationEffectiveDate = DateTime.UtcNow.AddDays(1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Should_Fail_When_Reason_Is_Empty()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "",
                CancellationEffectiveDate = DateTime.UtcNow.AddDays(1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Should_Fail_When_Reason_Is_Whitespace()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "   ",
                CancellationEffectiveDate = DateTime.UtcNow.AddDays(1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Reason");
        }

        [Fact]
        public void Should_Fail_When_CancellationEffectiveDate_Is_Default()
        {
            var validator = new CancelPolicyCommandValidator();
            var cmd = new CancelPolicyCommand
            {
                PolicyId = Guid.NewGuid(),
                Reason = "Fraud detected",
                CancellationEffectiveDate = default
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CancellationEffectiveDate" && e.ErrorMessage == "CancellationEffectiveDate is required.");
        }
    }
}