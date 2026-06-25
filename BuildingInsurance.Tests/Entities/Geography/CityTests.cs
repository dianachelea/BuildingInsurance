using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Tests.Entities.Geography
{
    public class CityTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() => new City(name!, Guid.NewGuid()));
            Assert.Contains("City name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenCountyIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() => new City("Bucuresti", Guid.Empty));
            Assert.Contains("County ID cannot be empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldUppercase_AndTrim_Name()
        {
            var city = new City("  Bucuresti  ", Guid.NewGuid());
            Assert.Equal("BUCURESTI", city.Name);
        }

        [Fact]
        public void Constructor_WithId_ShouldGenerateNewId_WhenEmptyIdProvided()
        {
            var city = new City(Guid.Empty, "Bucuresti", Guid.NewGuid());
            Assert.NotEqual(Guid.Empty, city.Id);
        }

        [Fact]
        public void Constructor_WithId_ShouldKeepProvidedId_WhenNotEmpty()
        {
            var id = Guid.NewGuid();
            var city = new City(id, "Bucuresti", Guid.NewGuid());
            Assert.Equal(id, city.Id);
        }
    }
}