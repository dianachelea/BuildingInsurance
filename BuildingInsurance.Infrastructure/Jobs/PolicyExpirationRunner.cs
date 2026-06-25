using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Infrastructure.Jobs
{
    public sealed class PolicyExpirationRunner : IPolicyExpirationService
    {
        private readonly BuildingInsuranceDbContext _db;
        private readonly ILogger<PolicyExpirationRunner> _logger;

        public PolicyExpirationRunner(BuildingInsuranceDbContext db, ILogger<PolicyExpirationRunner> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct = default)
        {
            if (nowUtc.Kind != DateTimeKind.Utc)
                nowUtc = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);

            var updated = await _db.Policies
                .Where(p => p.PolicyStatus == PolicyStatus.Active && p.EndDate < nowUtc)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(p => p.PolicyStatus, PolicyStatus.Expired), ct);

            _logger.LogInformation("Expire run at {NowUtc}. Updated={Updated}", nowUtc, updated);
            return updated;
        }
    }
}