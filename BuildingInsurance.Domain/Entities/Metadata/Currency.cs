using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Metadata
{
    public class Currency : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public string Code { get; private set; } = null!;
        public string Name { get; private set; } = null!;
        public decimal ExchangeRateToBase { get; private set; }
        public bool IsActive { get; private set; }

        private Currency() { }

        public Currency(Guid id, string code, string name, decimal exchangeRateToBase, bool isActive)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetCode(code);
            SetName(name);
            SetExchangeRateToBase(exchangeRateToBase);
            IsActive = isActive;
        }
        public Currency(string code, string name, decimal exchangeRateToBase, bool isActive)
        {
            Id = Guid.NewGuid();
            SetCode(code);
            SetName(name);
            SetExchangeRateToBase(exchangeRateToBase);
            IsActive = isActive;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void UpdateName(string name)
        {
            SetName(name);
        }

        public void UpdateExchangeRateToBase(decimal exchangeRateToBase)
        {
            SetExchangeRateToBase(exchangeRateToBase);
        }

        private void SetExchangeRateToBase(decimal exchangeRateToBase)
        {
            if(exchangeRateToBase <= 0)
                throw new ArgumentException("Exchange rate must be greater than zero.", nameof(exchangeRateToBase));

            ExchangeRateToBase = exchangeRateToBase;
        }

        private void SetName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Currency name cannot be null or empty.", nameof(name));

            Name = name.Trim();
        }

        private void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code is required.");

            code = code.Trim().ToUpperInvariant();

            if (code.Length != 3)
                throw new ArgumentException("Currency code must be exactly 3 characters.");

            Code = code;
        }
    }
}