using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Geography
{
    public class County : IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid CountryId { get; private set; }

        private County() { }

        public County(Guid id, string name, Guid countryId)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetName(name);
            SetCountryId(countryId);
        }

        public County(string name, Guid countryId)
        {
            Id = Guid.NewGuid();
            SetName(name);
            SetCountryId(countryId);
        }

        private void SetName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("County name cannot be null or empty.", nameof(name));

            Name = name.Trim().ToUpperInvariant();
        }

        private void SetCountryId(Guid countryId)
        {
            if (countryId == Guid.Empty)
                throw new ArgumentException("Country ID cannot be empty.", nameof(countryId));

            CountryId = countryId;
        }
    }
}