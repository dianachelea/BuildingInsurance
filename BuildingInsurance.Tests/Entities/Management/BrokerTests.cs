using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.Entities.Management
{
    public class BrokerTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenBrokerCodeMissing(string? code)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Broker(
                    brokerCode: code!,
                    name: "Test Broker",
                    contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                    brokerStatus: BrokerStatus.Active,
                    commissionPercentage: 0.10m));

            Assert.Contains("Broker code is required", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenNameMissing(string? name)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Broker(
                    brokerCode: "BRK01",
                    name: name!,
                    contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                    brokerStatus: BrokerStatus.Active,
                    commissionPercentage: 0.10m));

            Assert.Contains("Broker name is required", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenContactInfoIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Broker(
                    brokerCode: "BRK01",
                    name: "Test Broker",
                    contactInfo: null!,
                    brokerStatus: BrokerStatus.Active,
                    commissionPercentage: 0.10m));
        }

        [Fact]
        public void Constructor_ShouldTrimAndUppercaseBrokerCode()
        {
            var broker = new Broker(
                brokerCode: "  brk01  ",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            Assert.Equal("BRK01", broker.BrokerCode);
        }

        [Fact]
        public void Constructor_ShouldTrimName()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "  Test Broker  ",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            Assert.Equal("Test Broker", broker.FullName);
        }

        [Fact]
        public void Constructor_ShouldGenerateId_WhenEmptyGuidProvided()
        {
            var broker = new Broker(
                id: Guid.Empty,
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            Assert.NotEqual(Guid.Empty, broker.Id);
        }

        [Fact]
        public void Constructor_ShouldKeepProvidedId_WhenNonEmptyGuid()
        {
            var id = Guid.NewGuid();

            var broker = new Broker(
                id: id,
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            Assert.Equal(id, broker.Id);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(1.5)]
        public void Constructor_ShouldThrow_WhenCommissionOutOfRange(double value)
        {
            var commission = (decimal)value;

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new Broker(
                    brokerCode: "BRK01",
                    name: "Test Broker",
                    contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                    brokerStatus: BrokerStatus.Active,
                    commissionPercentage: commission));

            Assert.Contains("CommissionPercentage must be between 0 (exclusive) and 1 (exclusive)", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldAllowNullCommission()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: null);

            Assert.Null(broker.CommissionPercentage);
        }

        [Fact]
        public void Constructor_ShouldAllowValidCommission()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.25m);

            Assert.Equal(0.25m, broker.CommissionPercentage);
        }

        [Fact]
        public void Activate_ShouldSetStatusToActive()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Inactive,
                commissionPercentage: 0.10m);

            broker.Activate();

            Assert.Equal(BrokerStatus.Active, broker.BrokerStatus);
        }

        [Fact]
        public void Deactivate_ShouldSetStatusToInactive()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            broker.Deactivate();

            Assert.Equal(BrokerStatus.Inactive, broker.BrokerStatus);
        }

        [Fact]
        public void UpdateContact_ShouldThrow_WhenNull()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            Assert.Throws<ArgumentNullException>(() => broker.UpdateContact(null!));
        }

        [Fact]
        public void UpdateContact_ShouldReplaceContactInfo()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            var newContact = new ContactInfo("new@mail.com", "0799999999");

            broker.UpdateContact(newContact);

            Assert.Equal("new@mail.com", broker.ContactInfo.Email);
            Assert.Equal("0799999999", broker.ContactInfo.Phone);
        }

        [Fact]
        public void UpdateName_ShouldThrow_WhenInvalid()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            var ex = Assert.Throws<ArgumentException>(() => broker.UpdateName("   "));
            Assert.Contains("Broker name is required", ex.Message);
        }

        [Fact]
        public void UpdateName_ShouldTrimName()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            broker.UpdateName("  New Name  ");

            Assert.Equal("New Name", broker.FullName);
        }

        [Fact]
        public void UpdateCommission_ShouldAllowNull()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            broker.UpdateCommission(null);

            Assert.Null(broker.CommissionPercentage);
        }

        [Fact]
        public void UpdateCommission_ShouldThrow_WhenOutOfRange()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: 0.10m);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => broker.UpdateCommission(1.0m));

            Assert.Contains("CommissionPercentage must be between 0 (exclusive) and 1 (exclusive)", ex.Message);
        }

        [Fact]
        public void UpdateCommission_ShouldSetValidValue()
        {
            var broker = new Broker(
                brokerCode: "BRK01",
                name: "Test Broker",
                contactInfo: new ContactInfo("broker@example.com", "0712345678"),
                brokerStatus: BrokerStatus.Active,
                commissionPercentage: null);

            broker.UpdateCommission(0.15m);

            Assert.Equal(0.15m, broker.CommissionPercentage);
        }
    }
}