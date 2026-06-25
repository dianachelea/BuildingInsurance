using BuildingInsurance.Domain.Entities.Metadata;

namespace BuildingInsurance.Tests.Entities.Metadata
{
    public class CurrencyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Currency(
                    id: Guid.NewGuid(),
                    code: "RON",
                    name: name!,
                    exchangeRateToBase: 4.5m,
                    isActive: true));

            Assert.Contains("Currency name cannot be null or empty", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-0.001)]
        public void Constructor_ShouldThrow_WhenExchangeRateLessOrEqualZero(decimal rate)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Currency(
                    id: Guid.NewGuid(),
                    code: "EUR",
                    name: "Euro",
                    exchangeRateToBase: rate,
                    isActive: true));

            Assert.Contains("Exchange rate must be greater than zero", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldTrimName()
        {
            var currency = new Currency(
                id: Guid.NewGuid(),
                code: "EUR",
                name: "  Euro  ",
                exchangeRateToBase: 4.5m,
                isActive: true);

            Assert.Equal("Euro", currency.Name);
        }

        [Fact]
        public void Constructor_ShouldGenerateId_WhenEmptyGuidProvided()
        {
            var currency = new Currency(
                id: Guid.Empty,
                code: "RON",
                name: "Leu",
                exchangeRateToBase: 1m,
                isActive: true);

            Assert.NotEqual(Guid.Empty, currency.Id);
        }

        [Fact]
        public void Constructor_ShouldKeepProvidedId_WhenNonEmptyGuid()
        {
            var id = Guid.NewGuid();

            var currency = new Currency(
                id: id,
                code: "RON",
                name: "Leu",
                exchangeRateToBase: 1m,
                isActive: true);

            Assert.Equal(id, currency.Id);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            var currency = new Currency(
                id: Guid.NewGuid(),
                code: "USD",
                name: "Dollar",
                exchangeRateToBase: 4m,
                isActive: false);

            currency.Activate();

            Assert.True(currency.IsActive);
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            var currency = new Currency(
                id: Guid.NewGuid(),
                code: "EUR",
                name: "Euro",
                exchangeRateToBase: 4.5m,
                isActive: true);

            currency.Deactivate();

            Assert.False(currency.IsActive);
        }
    }
}