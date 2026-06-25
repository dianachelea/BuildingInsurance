using BuildingInsurance.Domain.Common;
using BuildingInsurance.Domain.Constants;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Events;
using BuildingInsurance.Domain.Interfaces;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Domain.Entities.Clients
{
    public class Client : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public ClientType Type { get; private set; }
        public string FullName { get; private set; } = null!;
        public string? PersonalIdentificationNumber { get; private set; }
        public string? CompanyRegistrationNumber { get; private set; }
        public ContactInfo ContactInfo { get; private set; } = null!;

        private Client() { }

        public Client(Guid id, ClientType type, string fullName, ContactInfo contactInfo, string? personalIdentificationNumber = null, string? companyRegistrationNumber = null)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetType(type);
            SetFullName(fullName);
            SetContactInfo(contactInfo);
            SetIdentificationNumbers(type, personalIdentificationNumber, companyRegistrationNumber);
        }

        public Client(ClientType type, string fullName, ContactInfo contactInfo, string? personalIdentificationNumber = null, string? companyRegistrationNumber = null)
        {
            Id = Guid.NewGuid();
            SetType(type);
            SetFullName(fullName);
            SetContactInfo(contactInfo);
            SetIdentificationNumbers(type, personalIdentificationNumber, companyRegistrationNumber);
        }

        private void SetType(ClientType type)
        {
            if (!Enum.IsDefined(typeof(ClientType), type))
                throw new ArgumentException("Invalid client type.", nameof(type));

            Type = type;
        }

        public void UpdateFullName(string fullName) => SetFullName(fullName);

        public void UpdateContactInfo(ContactInfo contactInfo) => SetContactInfo(contactInfo);
        
        public void ChangeIdentifier(string newIdentifier, string reason)
        {
            if (string.IsNullOrWhiteSpace(newIdentifier))
                throw new ArgumentException("New identifier is required.", nameof(newIdentifier));

            if (!ReasonNormalizer.TryNormalize(reason, IdentificationChangeReasons.Allowed, out var normalizedReason))
                throw new InvalidOperationException("Invalid identification change reason.");

            var trimmed = newIdentifier.Trim();

            if (Type == ClientType.Individual)
            {
                var old = PersonalIdentificationNumber ?? string.Empty;

                if (old == trimmed)
                    return;

                PersonalIdentificationNumber = trimmed;
                CompanyRegistrationNumber = null;

                AddDomainEvent(new ClientIdentifierChanged(Id, old, trimmed, normalizedReason));
                return;
            }

            if (Type == ClientType.Company)
            {
                var old = CompanyRegistrationNumber ?? string.Empty;

                if (old == trimmed)
                    return;

                CompanyRegistrationNumber = trimmed;
                PersonalIdentificationNumber = null;

                AddDomainEvent(new ClientIdentifierChanged(Id, old, trimmed, normalizedReason));
                return;
            }

            throw new InvalidOperationException("Unsupported client type.");
        }

        private void SetIdentificationNumbers(ClientType type, string? personalIdentificationNumber, string? companyRegistrationNumber)
        {
            if (type == ClientType.Individual)
            {
                if (string.IsNullOrWhiteSpace(personalIdentificationNumber))
                    throw new ArgumentException("Personal Identification Number is required for individual clients.", nameof(personalIdentificationNumber));

                PersonalIdentificationNumber = personalIdentificationNumber.Trim();
                CompanyRegistrationNumber = null;
            }
            else if (type == ClientType.Company)
            {
                if (string.IsNullOrWhiteSpace(companyRegistrationNumber))
                    throw new ArgumentException("Company Registration Number is required for company clients.", nameof(companyRegistrationNumber));
                CompanyRegistrationNumber = companyRegistrationNumber.Trim(); 

                PersonalIdentificationNumber = null;
            }
            else
            {
                throw new ArgumentException("Invalid client type for setting identification numbers.", nameof(type));
            }
        }

        private void SetContactInfo(ContactInfo contactInfo)
        {
            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo), "Contact info is required.");
        }

        private void SetFullName(string fullName)
        {
            if(string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be null or empty.", nameof(fullName));

            FullName = fullName.Trim();
        }
    }
}