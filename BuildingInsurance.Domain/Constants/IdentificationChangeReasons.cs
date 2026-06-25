namespace BuildingInsurance.Domain.Constants
{
    public static class IdentificationChangeReasons
    {
        public static readonly string[] Allowed =
        {
            "Typo correction",
            "Client provided new document",
            "Data migration fix",
            "Legal entity update",
            "Duplicate client merge",
            "Support request"
        };
    }
}