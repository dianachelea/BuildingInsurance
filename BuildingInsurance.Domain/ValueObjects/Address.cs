namespace BuildingInsurance.Domain.ValueObjects
{
    public class Address
    {
        public string Street { get; private set; } = null!;
        public string Number { get; private set; } = null!;

        private Address() { }

        public Address(string street, string number)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required.", nameof(street));
            
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Number is required.", nameof(number));

            Street = street.Trim().ToUpperInvariant();
            Number = number.Trim().ToUpperInvariant();
        }

        public override bool Equals(object? obj)
        {
            var other = obj as Address;
            if (other == null)
                return false;
            if (Street == other.Street && Number == other.Number)
            {
                return true;
            }
            return false;
        }

        public static bool operator ==(Address? left, Address? right) => Equals(left, right);
        public static bool operator !=(Address? left, Address? right) => !Equals(left, right);
        public override int GetHashCode()
        {
            return HashCode.Combine(Street, Number);
        }
    }
}