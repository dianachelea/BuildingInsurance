using BuildingInsurance.Application.Features.Brokers.Policies.Commands.ActivatePolicy;

namespace BuildingInsurance.Tests.Validators.Policy
{
    public sealed class ActivatePolicyCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new ActivatePolicyCommandValidator();
            var cmd = new ActivatePolicyCommand(Guid.NewGuid());

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_PolicyId_Is_Empty()
        {
            var validator = new ActivatePolicyCommandValidator();
            var cmd = new ActivatePolicyCommand(Guid.Empty);

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "PolicyId" && e.ErrorMessage == "PolicyId is required.");
        }
    }
}