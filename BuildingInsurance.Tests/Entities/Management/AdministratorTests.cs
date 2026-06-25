using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.Entities.Management
{
    public class AdministratorTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenFullNameMissing(string? fullName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Administrator(
                    fullName: fullName!,
                    contactInfo: new ContactInfo("admin@example.com", "0712345678"),
                    adminRole: AdminRole.Admin));

            Assert.Contains("Full name is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenContactInfoIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Administrator(
                    fullName: "Jane Admin",
                    contactInfo: null!,
                    adminRole: AdminRole.Admin));
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenAdminRoleInvalid()
        {
            var invalidRole = (AdminRole)999;

            var ex = Assert.Throws<ArgumentException>(() =>
                new Administrator(
                    fullName: "Jane Admin",
                    contactInfo: new ContactInfo("admin@example.com", "0712345678"),
                    adminRole: invalidRole));

            Assert.Contains("Invalid admin role", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldTrimFullName_AndGenerateId_WhenEmptyGuid()
        {
            var admin = new Administrator(
                id: Guid.Empty,
                fullName: "  Jane Admin  ",
                contactInfo: new ContactInfo("admin@example.com", "0712345678"),
                adminRole: AdminRole.Manager);

            Assert.Equal("Jane Admin", admin.FullName);
            Assert.NotEqual(Guid.Empty, admin.Id);
        }

        [Fact]
        public void Constructor_ShouldKeepProvidedId_WhenNonEmptyGuid()
        {
            var id = Guid.NewGuid();

            var admin = new Administrator(
                id: id,
                fullName: "Jane Admin",
                contactInfo: new ContactInfo("admin@example.com", "0712345678"),
                adminRole: AdminRole.Admin);

            Assert.Equal(id, admin.Id);
        }
    }
}