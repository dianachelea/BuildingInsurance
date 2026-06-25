using BuildingInsurance.Application.Features.Brokers.Policies.Commands.CreateDraftPolicy;

namespace BuildingInsurance.Tests.Validators.Policy
{
    public sealed class CreateDraftPolicyCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.Empty,
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ClientId" && e.ErrorMessage == "ClientId is required.");
        }

        [Fact]
        public void Should_Fail_When_BuildingId_Is_Empty()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.Empty,
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BuildingId" && e.ErrorMessage == "BuildingId is required.");
        }

        [Fact]
        public void Should_Fail_When_CurrencyId_Is_Empty()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.Empty,
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CurrencyId" && e.ErrorMessage == "CurrencyId is required.");
        }

        [Fact]
        public void Should_Fail_When_BrokerId_Is_Empty()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.Empty,
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BrokerId" && e.ErrorMessage == "BrokerId is required.");
        }

        [Fact]
        public void Should_Fail_When_BasePremium_Is_Zero()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 0m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BasePremium" && e.ErrorMessage == "BasePremium must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_BasePremium_Is_Negative()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = -1m,
                StartDate = DateTime.UtcNow.Date.AddDays(1),
                EndDate = DateTime.UtcNow.Date.AddDays(366)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BasePremium" && e.ErrorMessage == "BasePremium must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_EndDate_Is_Before_StartDate()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand()
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(9)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "EndDate must be after StartDate.");
        }

        [Fact]
        public void Should_Fail_When_EndDate_Is_Equal_To_StartDate()
        {
            var validator = new CreateDraftPolicyCommandValidator();
            var cmd = new CreateDraftPolicyCommand() 
            {
                ClientId = Guid.NewGuid(),
                BuildingId = Guid.NewGuid(),
                CurrencyId = Guid.NewGuid(),
                BrokerId = Guid.NewGuid(),
                BasePremium = 100m,
                StartDate = DateTime.UtcNow.Date.AddDays(10),
                EndDate = DateTime.UtcNow.Date.AddDays(10)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "EndDate" && e.ErrorMessage == "EndDate must be after StartDate.");
        }
    }
}