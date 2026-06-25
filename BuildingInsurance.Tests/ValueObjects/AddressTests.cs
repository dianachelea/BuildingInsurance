using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.ValueObjects
{
    public class AddressTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenStreetMissing(string? street)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Address(street!, "10A"));
            Assert.Contains("Street is required", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNumberMissing(string? number)
        {
            var ex = Assert.Throws<ArgumentException>(() => new Address("Strada Exemplu", number!));
            Assert.Contains("Number is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldTrim_AndUppercase_Street_AndNumber()
        {
            var address = new Address("  Strada exemplu  ", " 10a ");

            Assert.Equal("STRADA EXEMPLU", address.Street);
            Assert.Equal("10A", address.Number);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForSameStreetAndNumber()
        {
            var a1 = new Address("Strada Exemplu", "10A");
            var a2 = new Address("  strada exemplu  ", " 10a ");

            Assert.True(a1.Equals(a2));
            Assert.True(a1 == a2);
            Assert.False(a1 != a2);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentStreet()
        {
            var a1 = new Address("Strada exemplu", "10A");
            var a2 = new Address("Alta strada", "10A");

            Assert.False(a1.Equals(a2));
            Assert.False(a1 == a2);
            Assert.True(a1 != a2);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentNumber()
        {
            var a1 = new Address("Strada exemplu", "10A");
            var a2 = new Address("Strada exemplu", "11");

            Assert.False(a1.Equals(a2));
            Assert.False(a1 == a2);
            Assert.True(a1 != a2);
        }

        [Fact]
        public void OperatorEquals_ShouldReturnTrue_WhenBothNull()
        {
            Address? a1 = null;
            Address? a2 = null;

            Assert.True(a1 == a2);
            Assert.False(a1 != a2);
        }

        [Fact]
        public void OperatorEquals_ShouldReturnFalse_WhenOneNull()
        {
            Address? a1 = null;
            var a2 = new Address("Strada exemplu", "10A");

            Assert.False(a1 == a2);
            Assert.True(a1 != a2);
        }

        [Fact]
        public void GetHashCode_ShouldBeSame_ForEqualAddresses()
        {
            var a1 = new Address("Strada exemplu", "10A");
            var a2 = new Address("  STRADA EXEMPLU ", " 10a ");

            Assert.Equal(a1.GetHashCode(), a2.GetHashCode());
        }
    }
}