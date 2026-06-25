using BuildingInsurance.Domain.Entities.Geography;

namespace BuildingInsurance.Tests.Entities.Geography
{
    public class CountyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() => new County(name!, Guid.NewGuid()));
            Assert.Contains("County name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenCountryIdIsEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() => new County("Ilfov", Guid.Empty));
            Assert.Contains("Country ID cannot be empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldUppercase_AndTrim_Name()
        {
            var county = new County("  Ilfov  ", Guid.NewGuid());
            Assert.Equal("ILFOV", county.Name);
        }

        [Fact]
        public void Constructor_WithId_ShouldGenerateNewId_WhenEmptyIdProvided()
        {
            var county = new County(Guid.Empty, "Ilfov", Guid.NewGuid());
            Assert.NotEqual(Guid.Empty, county.Id);
        }

        [Fact]
        public void Constructor_WithId_ShouldKeepProvidedId_WhenNotEmpty()
        {
            var id = Guid.NewGuid();
            var county = new County(id, "Ilfov", Guid.NewGuid());
            Assert.Equal(id, county.Id);
        }
    }
}