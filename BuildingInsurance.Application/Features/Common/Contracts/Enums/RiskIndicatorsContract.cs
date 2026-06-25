namespace BuildingInsurance.Application.Features.Common.Contracts.Enums
{
    [Flags]
    public enum RiskIndicatorsContract
    {
        None = 0,
        FloodZone = 1,
        EarthquakeProne = 2
    }
}