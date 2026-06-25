using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Events;
using BuildingInsurance.Domain.Interfaces;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Domain.Entities.Management
{
    public class Broker : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public string BrokerCode { get; private set; } = null!;
        public string FullName { get; private set; } = null!;
        public ContactInfo ContactInfo { get; private set; } = null!;
        public BrokerStatus BrokerStatus { get; private set; }
        public decimal? CommissionPercentage { get; private set; }

        private Broker() { }

        public Broker(Guid id, string brokerCode, string name, ContactInfo contactInfo, BrokerStatus brokerStatus, decimal? commissionPercentage)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetBrokerCode(brokerCode);
            SetName(name);
            SetContactInfo(contactInfo);
            SetBrokerStatus(brokerStatus);
            SetCommissionPercentage(commissionPercentage);
        }

        public Broker(string brokerCode, string name, ContactInfo contactInfo, BrokerStatus brokerStatus, decimal? commissionPercentage)
        {
            Id = Guid.NewGuid();
            SetBrokerCode(brokerCode);
            SetName(name);
            SetContactInfo(contactInfo);
            SetBrokerStatus(brokerStatus);
            SetCommissionPercentage(commissionPercentage);
        }

        public void Activate()
        {
            if (BrokerStatus == BrokerStatus.Active)
                return;

            SetBrokerStatus(BrokerStatus.Active);
            AddDomainEvent(new BrokerActivated(Id));
        }

        public void MarkAsCreated()
        {
            if (DomainEvents.OfType<BrokerCreated>().Any())
                return;

            AddDomainEvent(new BrokerCreated(Id, BrokerCode));
        }

        public void Deactivate()
        {
            if (BrokerStatus == BrokerStatus.Inactive)
                return;

            SetBrokerStatus(BrokerStatus.Inactive);
            AddDomainEvent(new BrokerDeactivated(Id));
        }

        public void UpdateContact(ContactInfo contactInfo)
        {
            SetContactInfo(contactInfo);
        }

        public void UpdateName(string name)
        {
            SetName(name);
        }

        public void UpdateCommission(decimal? commissionPercentage)
        {
            SetCommissionPercentage(commissionPercentage);
        }

        private void SetCommissionPercentage(decimal? commissionPercentage)
        {
            if (commissionPercentage is not null && (commissionPercentage <= 0m || commissionPercentage >= 1m))
                throw new ArgumentOutOfRangeException(nameof(commissionPercentage), "CommissionPercentage must be between 0 (exclusive) and 1 (exclusive).");

            CommissionPercentage = commissionPercentage;
        }

        private void SetBrokerStatus(BrokerStatus brokerStatus)
        {
            BrokerStatus = brokerStatus;
        }

        private void SetContactInfo(ContactInfo contactInfo)
        {
            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Broker name is required.", nameof(name));

            FullName = name.Trim();
        }

        private void SetBrokerCode(string brokerCode)
        {
            if (string.IsNullOrWhiteSpace(brokerCode))
                throw new ArgumentException("Broker code is required.", nameof(brokerCode));

            BrokerCode = brokerCode.Trim().ToUpperInvariant();
        }
    }
}