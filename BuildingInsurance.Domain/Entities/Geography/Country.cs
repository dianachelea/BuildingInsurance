using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Geography
{
    public class Country : IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;

        private Country() { }

        public Country(string name)
        {
            Id = Guid.NewGuid();
            SetName(name);
        }

        public Country(Guid id, string name)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetName(name);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Country name cannot be null or empty.", nameof(name));
            
            Name = name.Trim().ToUpperInvariant();
        }
    }
}