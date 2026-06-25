using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Events;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Tests.DomainEvents
{
    public class ClientIdentifierChangedTests
    {
        [Fact]
        public void ChangeIdentifier_ForIndividual_ShouldRaiseEvent_AndUpdateFields()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                personalIdentificationNumber: "RO123"
            );

            var newId = "RO999";
            var reason = "Support request";

            client.ChangeIdentifier(newId, reason);

            Assert.Equal("RO999", client.PersonalIdentificationNumber);
            Assert.Null(client.CompanyRegistrationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.NotNull(evt);

            Assert.Equal(client.Id, evt!.ClientId);
            Assert.Equal("RO123", evt.OldValue);
            Assert.Equal("RO999", evt.NewValue);
            Assert.Equal(reason, evt.Reason);
        }

        [Fact]
        public void ChangeIdentifier_ForCompany_ShouldRaiseEvent_AndUpdateFields()
        {
            var client = new Client(
                type: ClientType.Company,
                fullName: "ACME SRL",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                companyRegistrationNumber: "J40/123/2020"
            );

            var newId = "J40/999/2024";
            var reason = "Typo correction";

            client.ChangeIdentifier(newId, reason);

            Assert.Equal("J40/999/2024", client.CompanyRegistrationNumber);
            Assert.Null(client.PersonalIdentificationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.NotNull(evt);

            Assert.Equal(client.Id, evt!.ClientId);
            Assert.Equal("J40/123/2020", evt.OldValue);
            Assert.Equal("J40/999/2024", evt.NewValue);
            Assert.Equal(reason, evt.Reason);
        }

        [Fact]
        public void ChangeIdentifier_ForIndividual_SameValue_ShouldNotRaiseEvent()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                personalIdentificationNumber: "RO123"
            );

            client.ChangeIdentifier("RO123", "Data migration fix");

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.Null(evt);
        }

        [Fact]
        public void ChangeIdentifier_ForCompany_SameValue_ShouldNotRaiseEvent()
        {
            var client = new Client(
                type: ClientType.Company,
                fullName: "ACME SRL",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                companyRegistrationNumber: "J40/123/2020"
            );

            client.ChangeIdentifier("J40/123/2020", "Data migration fix");

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.Null(evt);
        }

        [Fact]
        public void ChangeIdentifier_EmptyNewIdentifier_ShouldThrowArgumentException()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                    email: "test@test.com", 
                    phone: "0712345678"
                    ),
                personalIdentificationNumber: "RO123"
            );

            Assert.Throws<ArgumentException>(() => client.ChangeIdentifier("   ", "Support request"));
        }

        [Fact]
        public void ChangeIdentifier_EmptyReason_ShouldThrowInvalidOperationException()
        {
            var client = new Client(
                type: ClientType.Company,
                fullName: "ACME SRL",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                companyRegistrationNumber: "J40/123/2020"
            );

            Assert.Throws<InvalidOperationException>(() => client.ChangeIdentifier("J40/999/2024", "   "));
        }

        [Fact]
        public void ChangeIdentifier_ShouldTrimNewIdentifier_AndStoreTrimmedValue()
        {
            var client = new Client(
                type: ClientType.Individual,
                fullName: "John Doe",
                contactInfo: new ContactInfo(
                            email: "test@test.com",
                            phone: "0712345678"
                            ),
                personalIdentificationNumber: "RO123"
            );

            client.ChangeIdentifier("  RO999  ", "Support request");

            Assert.Equal("RO999", client.PersonalIdentificationNumber);

            var evt = client.DomainEvents.OfType<ClientIdentifierChanged>().FirstOrDefault();
            Assert.NotNull(evt);
            Assert.Equal("RO999", evt!.NewValue);
        }
    }
}