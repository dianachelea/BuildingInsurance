using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Interfaces;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Domain.Entities.Buildings
{
    public class Building : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Address Address { get; private set; } = null!;
        public Guid CityId { get; private set; }
        public int ConstructionYear { get; private set; }
        public BuildingType Type { get; private set; }
        public int NumberOfFloors { get; private set; }
        public decimal SurfaceArea { get; private set; }
        public decimal InsuredValue { get; private set; }
        public RiskIndicators RiskIndicators { get; private set; }

        private Building() { }

        public Building(Guid id, Guid clientId, Address address, Guid cityId, int constructionYear, BuildingType type, int numberOfFloors, decimal surfaceArea, decimal insuredValue, RiskIndicators riskIndicators)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetClientId(clientId);
            SetAddress(address);
            SetCityId(cityId);
            SetConstructionYear(constructionYear);
            SetType(type);
            SetNumberOfFloors(numberOfFloors);
            SetSurfaceArea(surfaceArea);
            SetInsuredValue(insuredValue);
            SetRiskIndicators(riskIndicators);
        }
        
        public Building(Guid clientId, Address address, Guid cityId, int constructionYear, BuildingType type, int numberOfFloors, decimal surfaceArea, decimal insuredValue, RiskIndicators riskIndicators)
        {
            Id = Guid.NewGuid();
            SetClientId(clientId);
            SetAddress(address);
            SetCityId(cityId);
            SetConstructionYear(constructionYear);
            SetType(type);
            SetNumberOfFloors(numberOfFloors);
            SetSurfaceArea(surfaceArea);
            SetInsuredValue(insuredValue);
            SetRiskIndicators(riskIndicators);
        }
        
        public void AddRisk(RiskIndicators risk) => RiskIndicators |= risk;

        public void RemoveRisk(RiskIndicators risk) => RiskIndicators &= ~risk;

        public void ChangeAddress(Address newAddress)
        {
            SetAddress(newAddress);
        }

        public void Relocate(Address newAddress, Guid newCityId)
        {
            SetAddress(newAddress);
            SetCityId(newCityId);
        }

        public void UpdateConstruction(int constructionYear, BuildingType type, int floors, decimal surfaceArea)
        {
            SetConstructionYear(constructionYear);
            SetType(type);
            SetNumberOfFloors(floors);
            SetSurfaceArea(surfaceArea);
        }

        public void UpdateInsuredValue(decimal insuredValue) => SetInsuredValue(insuredValue);

        public void UpdateRiskIndicators(RiskIndicators riskIndicators) => SetRiskIndicators(riskIndicators);
        
        private void SetRiskIndicators(RiskIndicators riskIndicators)
        {
            if ((int)riskIndicators < 0)
                throw new ArgumentOutOfRangeException(nameof(riskIndicators));

            RiskIndicators = riskIndicators;
        }

        private void SetInsuredValue(decimal insuredValue)
        {
            if (insuredValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(insuredValue), "InsuredValue must be greater than 0.");

            InsuredValue = insuredValue;
        }

        private void SetSurfaceArea(decimal surfaceArea)
        {
            if (surfaceArea <= 0)
                throw new ArgumentOutOfRangeException(nameof(surfaceArea), "SurfaceArea must be greater than 0.");

            SurfaceArea = surfaceArea;
        }

        private void SetNumberOfFloors(int numberOfFloors)
        {
            if (numberOfFloors <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfFloors), "NumberOfFloors must be greater than 0.");

            NumberOfFloors = numberOfFloors;
        }

        private void SetType(BuildingType type)
        {
            if (!Enum.IsDefined(typeof(BuildingType), type))
                throw new ArgumentException("Invalid building type.", nameof(type));

            Type = type;
        }

        private void SetConstructionYear(int constructionYear)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (constructionYear < 1800 || constructionYear > currentYear)
                throw new ArgumentOutOfRangeException(nameof(constructionYear), $"ConstructionYear must be between 1800 and {currentYear}.");

            ConstructionYear = constructionYear;
        }

        private void SetCityId(Guid cityId)
        {
            if (cityId == Guid.Empty)
                throw new ArgumentException("CityId is required.", nameof(cityId));

            CityId = cityId;
        }

        private void SetAddress(Address address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address), "Address is required.");

            Address = address;
        }

        private void SetClientId(Guid clientId)
        {
            if (clientId == Guid.Empty)
                throw new ArgumentException("ClientId is required.", nameof(clientId));

            ClientId = clientId;
        }
    }
}