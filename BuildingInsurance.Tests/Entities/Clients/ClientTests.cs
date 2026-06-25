using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Events;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.Entities.Clients
{
    public class ClientTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ShouldThrow_WhenFullNameMissing(string? fullName)
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Client(
                    type: ClientType.Individual,
                    fullName: fullName!,
                    contactInfo: new ContactInfo(
                        email: "john@example.com",
                        phone: "0712345678",
                        address: new Address("Strada Exemplu", "10A")),
                    personalIdentificationNumber: "1234567890123"));

            Assert.Contains("Full name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenContactInfoIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Client(
                    type: ClientType.Individual,
                    fullName: "John Doe",
                    contactInfo: null!,
                    personalIdentificationNumber: "1234567890123"));
        }

        [Fact]
        public void Constructor_Individual_ShouldThrow_WhenPersonalIdentificationNumberMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Client(
                    type: ClientType.Individual,
                    fullName: "John Doe",
                    contactInfo: new ContactInfo(
                        email: "john@example.com",
                        phone: "0712345678"),
                    personalIdentificationNumber: null));

            Assert.Contains("Personal Identification Number is required", ex.Message);
        }

        [Fact]
        public void Constructor_Company_ShouldThrow_WhenCompanyRegistrationNumberMissing()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Client(
                    type: ClientType.Company,
                    fullName: "ACME SRL",
                    contactInfo: new ContactInfo(
                        email: "office@acme.com",
                        phone: "0211234567"),
                    companyRegistrationNumber: null));

            Assert.Contains("Company Registration Number is required", ex.Message);
        }

        [Fact]
        public void Constructor_Individual_ShouldSetPersonalIdentificationNumber_AndNullCompanyRegistrationNumber()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                    email: "john@example.com",
                    phone: "0712345678"),
                personalIdentificationNumber: " 1234567890123 ");

            Assert.Equal("1234567890123", client.PersonalIdentificationNumber);
            Assert.Null(client.CompanyRegistrationNumber);
        }

        [Fact]
        public void Constructor_Company_ShouldSetCompanyRegistrationNumber_AndNullPersonalIdentificationNumber()
        {
            var client = new Client(
                type: ClientType.Company,
                fullName: "ACME SRL",
                contactInfo: new ContactInfo(
                    email: "office@acme.com",
                    phone: "0211234567"),
                companyRegistrationNumber: " RO123 ");

            Assert.Equal("RO123", client.CompanyRegistrationNumber);
            Assert.Null(client.PersonalIdentificationNumber);
        }

        [Fact]
        public void UpdateFullName_ShouldThrow_WhenNameIsInvalid()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                    email: "john@example.com",
                    phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            var ex = Assert.Throws<ArgumentException>(() => client.UpdateFullName("   "));
            Assert.Contains("Full name cannot be null or empty", ex.Message);
        }

        [Fact]
        public void UpdateFullName_ShouldTrimName()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                    email: "john@example.com",
                    phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            client.UpdateFullName("  Jane Doe  ");

            Assert.Equal("Jane Doe", client.FullName);
        }

        [Fact]
        public void UpdateContactInfo_ShouldThrow_WhenNull()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                    email: "john@example.com",
                    phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            Assert.Throws<ArgumentNullException>(() => client.UpdateContactInfo(null!));
        }

        [Fact]
        public void ChangeIdentifier_ShouldThrow_WhenNewIdentifierMissing()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(email: "john@example.com", phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            var ex = Assert.Throws<ArgumentException>(() => client.ChangeIdentifier("   ", "correction"));
            Assert.Contains("New identifier is required", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ChangeIdentifier_ShouldThrow_WhenReasonMissing(string? reason)
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(email: "john@example.com", phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            var ex = Assert.Throws<InvalidOperationException>(() => client.ChangeIdentifier("9999999999999", reason!));
            Assert.Contains("Invalid identification change reason", ex.Message);
        }

        [Fact]
        public void ChangeIdentifier_Individual_ShouldUpdatePin_AndRaiseEvent()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(email: "john@example.com", phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            client.ChangeIdentifier(" 9999999999999 ", "Typo correction");

            Assert.Equal("9999999999999", client.PersonalIdentificationNumber);
            Assert.Null(client.CompanyRegistrationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.NotNull(evt);

            Assert.Equal(client.Id, evt!.ClientId);
            Assert.Equal("1234567890123", evt.OldValue);
            Assert.Equal("9999999999999", evt.NewValue);
            Assert.Equal("Typo correction", evt.Reason);
        }

        [Fact]
        public void ChangeIdentifier_Company_ShouldUpdateCompanyNumber_AndRaiseEvent()
        {
            var client = new Client(
                type: ClientType.Company,
                fullName: "ACME SRL",
                contactInfo: new ContactInfo(email: "office@acme.com", phone: "0211234567"),
                companyRegistrationNumber: "RO123");

            client.ChangeIdentifier(" RO999 ", "Legal entity update");

            Assert.Equal("RO999", client.CompanyRegistrationNumber);
            Assert.Null(client.PersonalIdentificationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.NotNull(evt);

            Assert.Equal(client.Id, evt!.ClientId);
            Assert.Equal("RO123", evt.OldValue);
            Assert.Equal("RO999", evt.NewValue);
            Assert.Equal("Legal entity update", evt.Reason);
        }

        [Fact]
        public void ChangeIdentifier_Individual_SameValue_ShouldNotRaiseEvent_AndShouldNotChangeAnything()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(email: "john@example.com", phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            client.ChangeIdentifier("1234567890123", "Support request");

            Assert.Equal("1234567890123", client.PersonalIdentificationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.Null(evt);
        }

        [Fact]
        public void ChangeIdentifier_ShouldAddOnlyOneEvent_PerChange()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(email: "john@example.com", phone: "0712345678"),
                personalIdentificationNumber: "1234567890123");

            client.ChangeIdentifier("9999999999999", "Data migration fix");

            var count = client.DomainEvents.OfType<ClientIdentifierChanged>().Count();
            Assert.Equal(1, count);
        }
    }
}