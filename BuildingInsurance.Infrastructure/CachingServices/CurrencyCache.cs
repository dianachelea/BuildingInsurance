using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingInsurance.Infrastructure.CachingServices
{
    public sealed class CurrencyCache : ICurrencyCachingService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private Dictionary<Guid, string> _codeById = new();
        private Dictionary<string, Guid> _idByCode = new(StringComparer.OrdinalIgnoreCase);

        public CurrencyCache(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LoadAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var currencies = await unitOfWork.Currencies.GetAllAsync(ct);

            _codeById = currencies.ToDictionary(x => x.Id, x => x.Code.ToString());
            _idByCode = currencies.ToDictionary(x => x.Code.ToString(), x => x.Id, StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetCode(Guid currencyId, out string code)
        {
            if (_codeById.TryGetValue(currencyId, out var v))
            {
                code = v;
                return true;
            }

            code = string.Empty;
            return false;
        }

        public bool TryGetId(string code, out Guid id)
        {
            return _idByCode.TryGetValue(code, out id);
        }
    }
}