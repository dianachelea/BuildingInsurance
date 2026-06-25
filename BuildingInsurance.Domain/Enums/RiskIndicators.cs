namespace BuildingInsurance.Domain.Enums
{
    [Flags]
    public enum RiskIndicators
    {
        None = 0,
        FloodZone = 1,
        EarthquakeProne = 2
    }
}