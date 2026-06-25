using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.CreateBuilding;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;

namespace BuildingInsurance.Tests.Validators.Building
{
    public class CreateBuildingCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.Empty,
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ClientId" && e.ErrorMessage == "Client ID is required.");
        }

        [Fact]
        public void Should_Fail_When_CityId_Is_Empty()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.Empty,
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CityId" && e.ErrorMessage == "CityId is required.");
        }

        [Fact]
        public void Should_Fail_When_Street_Is_Empty()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "   ",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Street" && e.ErrorMessage == "Address street is required.");
        }

        [Fact]
        public void Should_Fail_When_Street_Too_Long()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = new string('a', 201),
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Street" && e.ErrorMessage == "Address street must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Number_Is_Empty()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "   ",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Number" && e.ErrorMessage == "Address number is required.");
        }

        [Fact]
        public void Should_Fail_When_Number_Too_Long()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = new string('1', 21),
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Number" && e.ErrorMessage == "Address number must not exceed 20 characters.");
        }

        [Fact]
        public void Should_Fail_When_ConstructionYear_Is_Too_Old()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 1799,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ConstructionYear" && e.ErrorMessage == $"Construction year must be between 1800 and {DateTime.UtcNow.Year}.");
        }

        [Fact]
        public void Should_Fail_When_ConstructionYear_Is_In_The_Future()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = DateTime.UtcNow.Year + 1,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ConstructionYear" && e.ErrorMessage == $"Construction year must be between 1800 and {DateTime.UtcNow.Year}.");
        }

        [Fact]
        public void Should_Fail_When_NumberOfFloors_Is_Zero_Or_Negative()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 0,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "NumberOfFloors" && e.ErrorMessage == "Number of floors must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_SurfaceArea_Is_Zero_Or_Negative()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 0,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "SurfaceArea" && e.ErrorMessage == "Surface area must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_InsuredValue_Is_Zero_Or_Negative()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 0,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "InsuredValue" && e.ErrorMessage == "Insured value must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_Type_Is_Invalid()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = (BuildingTypeContract)999,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Type" && e.ErrorMessage == "Building type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_RiskIndicators_Is_Invalid_Negative()
        {
            var validator = new CreateBuildingCommandValidator();
            var cmd = new CreateBuildingCommand
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main Street",
                Number = "10",
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.50m,
                InsuredValue = 150000m,
                RiskIndicators = (RiskIndicatorsContract)(-1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "Risk indicators value is invalid.");
        }
    }
}