namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface IGeographyCachingService
    {
        bool TryGet(Guid cityId, out string city, out string county, out string country);
        Task LoadAsync(CancellationToken ct);
    }
}