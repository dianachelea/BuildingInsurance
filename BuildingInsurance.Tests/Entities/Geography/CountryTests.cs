using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Tests.Entities.Geography
{
    public class CountryTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Country(name!));
            Assert.Contains("Country name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldUppercase_AndTrim_Name()
        {
            var country = new Country("  Romania  ");

            Assert.Equal("ROMANIA", country.Name);
        }

        [Fact]
        public void Constructor_WithId_ShouldGenerateNewId_WhenEmptyIdProvided()
        {
            var country = new Country(Guid.Empty, "Romania");

            Assert.NotEqual(Guid.Empty, country.Id);
        }

        [Fact]
        public void Constructor_WithId_ShouldKeepProvidedId_WhenNotEmpty()
        {
            var id = Guid.NewGuid();
            var country = new Country(id, "Romania");

            Assert.Equal(id, country.Id);
        }
    }
}