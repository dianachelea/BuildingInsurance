using BuildingInsurance.Application.Features.Administrators.Brokers.Commands.ActivateBroker;

namespace BuildingInsurance.Tests.Validators.Broker
{
    public class ActivateBrokerCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new ActivateBrokerCommandValidator();
            var cmd = new ActivateBrokerCommand(Guid.NewGuid());

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_BrokerId_Is_Empty()
        {
            var validator = new ActivateBrokerCommandValidator();
            var cmd = new ActivateBrokerCommand(Guid.Empty);

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerId" && e.ErrorMessage == "BrokerId is required.");
        }
    }
}