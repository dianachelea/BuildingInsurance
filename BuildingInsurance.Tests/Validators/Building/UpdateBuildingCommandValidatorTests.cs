using BuildingInsurance.Application.Features.Brokers.Buildings.Commands.UpdateBuilding;
using BuildingInsurance.Application.Features.Common.Contracts.Enums;
using BuildingInsurance.Application.Features.Common.Dtos;

namespace BuildingInsurance.Tests.Validators.Building
{
    public class UpdateBuildingCommandValidatorTests
    {
        [Fact]
        public void Should_Pass_For_Valid_Command()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto
                {
                    Street = "Main Street",
                    Number = "10"
                },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 120.5m,
                InsuredValue = 150000m,
                RiskIndicators = RiskIndicatorsContract.FloodZone
            };

            var result = validator.Validate(cmd);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Fail_When_BuildingId_Is_Empty()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.Empty,
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "BuildingId" && e.ErrorMessage == "BuildingId is required.");
        }

        [Fact]
        public void Should_Fail_When_CityId_Is_Empty()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.Empty,
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "CityId" && e.ErrorMessage == "CityId is required.");
        }

        [Fact]
        public void Should_Fail_When_Address_Is_Null()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = null!,
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address" && e.ErrorMessage == "Address is required.");
        }

        [Fact]
        public void Should_Fail_When_Street_Is_Empty()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "   ", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Address street is required.");
        }

        [Fact]
        public void Should_Fail_When_Street_Too_Long()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto
                {
                    Street = new string('a', 201),
                    Number = "10"
                },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Street" && e.ErrorMessage == "Address street must not exceed 200 characters.");
        }

        [Fact]
        public void Should_Fail_When_Number_Is_Empty()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "   " },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Address.Number" && e.ErrorMessage == "Address number is required.");
        }

        [Fact]
        public void Should_Fail_When_ConstructionYear_Is_Too_Old()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 1700,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ConstructionYear");
        }

        [Fact]
        public void Should_Fail_When_ConstructionYear_Is_In_The_Future()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = DateTime.UtcNow.Year + 1,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "ConstructionYear");
        }

        [Fact]
        public void Should_Fail_When_Type_Is_Invalid()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = (BuildingTypeContract)999,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Type" && e.ErrorMessage == "Building type is invalid.");
        }

        [Fact]
        public void Should_Fail_When_NumberOfFloors_Is_Invalid()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 0,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "NumberOfFloors" && e.ErrorMessage == "Number of floors must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_SurfaceArea_Is_Invalid()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 0,
                InsuredValue = 100000,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "SurfaceArea" && e.ErrorMessage == "Surface area must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_InsuredValue_Is_Invalid()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 0,
                RiskIndicators = RiskIndicatorsContract.None
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "InsuredValue" && e.ErrorMessage == "Insured value must be greater than 0.");
        }

        [Fact]
        public void Should_Fail_When_RiskIndicators_Is_Negative()
        {
            var validator = new UpdateBuildingCommandValidator();
            var cmd = new UpdateBuildingCommand
            {
                BuildingId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Address = new AddressDto { Street = "Main", Number = "10" },
                ConstructionYear = 2005,
                Type = BuildingTypeContract.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100,
                InsuredValue = 100000,
                RiskIndicators = (RiskIndicatorsContract)(-1)
            };

            var result = validator.Validate(cmd);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "RiskIndicators" && e.ErrorMessage == "Risk indicators value is invalid.");
        }
    }
}