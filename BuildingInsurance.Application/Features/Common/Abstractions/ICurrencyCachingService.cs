namespace BuildingInsurance.Application.Features.Common.Abstractions
{
    public interface ICurrencyCachingService
    {
        Task LoadAsync(CancellationToken ct);
        bool TryGetCode(Guid currencyId, out string code);
        bool TryGetId(string code, out Guid id);
    }
}