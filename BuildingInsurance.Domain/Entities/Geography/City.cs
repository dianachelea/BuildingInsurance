using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Geography
{
    public class City : IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid CountyId { get; private set; }

        private City() { }

        public City(Guid id, string name, Guid countyId)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetName(name);
            SetCountyId(countyId);
        }

        public City(string name, Guid countyId)
        {
            Id = Guid.NewGuid();
            SetName(name);
            SetCountyId(countyId);
        }

        private void SetCountyId(Guid countyId)
        {
            if(countyId == Guid.Empty)
                throw new ArgumentException("County ID cannot be empty.", nameof(countyId));

            CountyId = countyId;
        }

        private void SetName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("City name cannot be null or empty.", nameof(name));

            Name = name.Trim().ToUpperInvariant();
        }
    }
}