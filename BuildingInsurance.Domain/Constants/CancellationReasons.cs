namespace BuildingInsurance.Domain.Constants
{
    public static class CancellationReasons
    {
        public static readonly string[] Allowed =
        {
            "Customer request",
            "Non-payment",
            "Underwriting decision",
            "Duplicate policy",
            "Fraud detected",
            "Policy expired and not renewed",
            "Administrative error",
            "Regulatory requirement"
        };
    }
}