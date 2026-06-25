using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.ValueObjects
{
    public class ContactInfoTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenEmailMissing(string? email)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new ContactInfo(email!, "0712345678"));

            Assert.Contains("Email is required", ex.Message);
        }


        [Theory]
        [InlineData("test")]
        [InlineData("test.com")]
        [InlineData("test@")]
        [InlineData("@test.com")]
        public void Constructor_ShouldThrow_WhenEmailInvalid(string email)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new ContactInfo(email, "0712345678"));

            Assert.Contains("Email is invalid", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenPhoneMissing(string? phone)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new ContactInfo("john@example.com", phone!));

            Assert.Contains("Phone is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldTrim_AndLowercase_Email_AndTrim_Phone()
        {
            var contact = new ContactInfo("  JOHN@EXAMPLE.COM  ", "  0712345678  ");

            Assert.Equal("john@example.com", contact.Email);
            Assert.Equal("0712345678", contact.Phone);
        }

        [Fact]
        public void Constructor_ShouldAllow_NullAddress()
        {
            var contact = new ContactInfo("john@example.com", "0712345678", address: null);

            Assert.Null(contact.Address);
        }

        [Fact]
        public void Equals_ShouldReturnTrue_ForSameEmailPhoneAndAddress()
        {
            var address1 = new Address("Strada Exemplu", "10A");
            var address2 = new Address("  strada exemplu ", " 10a ");

            var c1 = new ContactInfo("JOHN@EXAMPLE.COM", "0712345678", address1);
            var c2 = new ContactInfo("  john@example.com ", " 0712345678 ", address2);

            Assert.True(c1.Equals(c2));
            Assert.True(c1 == c2);
            Assert.False(c1 != c2);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentEmail()
        {
            var c1 = new ContactInfo("john@example.com", "0712345678");
            var c2 = new ContactInfo("jane@example.com", "0712345678");

            Assert.False(c1.Equals(c2));
            Assert.False(c1 == c2);
            Assert.True(c1 != c2);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentPhone()
        {
            var c1 = new ContactInfo("john@example.com", "0712345678");
            var c2 = new ContactInfo("john@example.com", "0799999999");

            Assert.False(c1.Equals(c2));
            Assert.False(c1 == c2);
            Assert.True(c1 != c2);
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentAddress()
        {
            var c1 = new ContactInfo("john@example.com", "0712345678", new Address("Strada Exemplu", "10A"));
            var c2 = new ContactInfo("john@example.com", "0712345678", new Address("Alta Strada", "10A"));

            Assert.False(c1.Equals(c2));
            Assert.False(c1 == c2);
            Assert.True(c1 != c2);
        }

        [Fact]
        public void OperatorEquals_ShouldReturnTrue_WhenBothNull()
        {
            ContactInfo? c1 = null;
            ContactInfo? c2 = null;

            Assert.True(c1 == c2);
            Assert.False(c1 != c2);
        }

        [Fact]
        public void OperatorEquals_ShouldReturnFalse_WhenOneNull()
        {
            ContactInfo? c1 = null;
            var c2 = new ContactInfo("john@example.com", "0712345678");

            Assert.False(c1 == c2);
            Assert.True(c1 != c2);
        }

        [Fact]
        public void GetHashCode_ShouldBeSame_ForEqualContactInfos()
        {
            var c1 = new ContactInfo("john@example.com", "0712345678", new Address("Strada Exemplu", "10A"));
            var c2 = new ContactInfo(" JOHN@EXAMPLE.COM ", " 0712345678 ", new Address("strada exemplu", "10a"));

            Assert.Equal(c1.GetHashCode(), c2.GetHashCode());
        }
    }
}