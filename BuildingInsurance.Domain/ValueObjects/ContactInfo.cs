namespace BuildingInsurance.Domain.ValueObjects
{
    public class ContactInfo
    {
        public string Email { get; private set; } = null!;
        public string Phone { get; private set; } = null!;
        public Address? Address { get; private set; }

        private ContactInfo() { }

        public ContactInfo(string email, string phone, Address? address=null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@'))
                throw new ArgumentException("Email is invalid.", nameof(email));

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone is required.", nameof(phone));

            Email = email.Trim().ToLowerInvariant();
            Phone = phone.Trim();
            Address = address;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as ContactInfo;

            if (other == null)
                return false;

            if (Email == other.Email && Phone == other.Phone && Address == other.Address)
            {
                return true;
            }
            return false;
        }

        public static bool operator ==(ContactInfo? left, ContactInfo? right) => Equals(left, right);

        public static bool operator !=(ContactInfo? left, ContactInfo? right) => !Equals(left, right);

        public override int GetHashCode()
        {
            return HashCode.Combine(Email, Phone, Address);
        }
    }
}