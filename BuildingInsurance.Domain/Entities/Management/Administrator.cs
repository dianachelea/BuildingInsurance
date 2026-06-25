using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Interfaces;
using BuildingInsurance.Domain.ValueObjects;

namespace BuildingInsurance.Domain.Entities.Management
{
    public class Administrator : IHasId
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; } = null!;
        public ContactInfo ContactInfo { get; private set; } = null!;
        public AdminRole AdminRole { get; private set; }

        private Administrator() { }

        public Administrator(Guid id, string fullName, ContactInfo contactInfo, AdminRole adminRole)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            SetFullName(fullName);
            SetContactInfo(contactInfo);
            SetAdminRole(adminRole);
        }

        public Administrator(string fullName, ContactInfo contactInfo, AdminRole adminRole)
        {
            Id = Guid.NewGuid();
            SetFullName(fullName);
            SetContactInfo(contactInfo);
            SetAdminRole(adminRole);
        }

        private void SetAdminRole(AdminRole adminRole)
        {
            if (!Enum.IsDefined(typeof(AdminRole), adminRole))
                throw new ArgumentException("Invalid admin role.", nameof(adminRole));

            AdminRole = adminRole;
        }

        private void SetContactInfo(ContactInfo contactInfo)
        {
            ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        }

        private void SetFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is required.", nameof(fullName));

            FullName = fullName.Trim();
        }
    }
}