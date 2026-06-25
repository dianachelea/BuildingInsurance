using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Application.Features.Common.Abstractions;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Infrastructure.Persistence;
using BuildingInsurance.Infrastructure.Persistence.Cursors;
using BuildingInsurance.Infrastructure.Persistence.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BuildingInsurance.Infrastructure.Jobs
{
    public sealed class PolicyReportFactsMaterializer : IPolicyReportFactsMaterializer
    {
        private const string ProcessName = "PolicyReportFacts";
        private const int BatchSize = 1000;

        private readonly IUnitOfWork _unitOfWork;
        private readonly BuildingInsuranceDbContext _db;
        private readonly ILogger<PolicyReportFactsMaterializer> _logger;

        public PolicyReportFactsMaterializer(IUnitOfWork unitOfWork, BuildingInsuranceDbContext db, ILogger<PolicyReportFactsMaterializer> logger)
        {
            _unitOfWork = unitOfWork;
            _db = db;
            _logger = logger;
        }

        public async Task<int> RunOnceAsync(DateTime nowUtc, CancellationToken ct)
        {
            bool transactionStarted = false;
            bool committed = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                transactionStarted = true;

                var checkpoint = await GetOrCreateCheckpointAsync(ct);

                var changed = await GetChangedPoliciesAsync(checkpoint.LastProcessedChangeVersion, ct);

                if (changed.Count == 0)
                {
                    await _unitOfWork.CommitAsync(ct);
                    committed = true;

                    _logger.LogInformation("PolicyReportFactsMaterializer: no changes. Cursor={Cursor}", checkpoint.LastProcessedChangeVersion);

                    return 0;
                }

                var (inserts, updates) = await UpsertPolicyReportFactsAsync(changed, ct);

                checkpoint.LastProcessedChangeVersion = changed[^1].ChangeVersion;

                await _unitOfWork.CommitAsync(ct);
                committed = true;

                _logger.LogInformation("PolicyReportFactsMaterializer ran. Batch={BatchCount} Inserts={Inserts} Updates={Updates} Cursor={Cursor}", changed.Count, inserts, updates, checkpoint.LastProcessedChangeVersion);

                return inserts + updates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while materializing PolicyReportFacts.");
                throw;
            }
            finally
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        await _unitOfWork.RollbackAsync(ct);
                    }
                    catch (Exception rbEx)
                    {
                        _logger.LogWarning(rbEx, "Rollback failed in PolicyReportFactsMaterializer.");
                    }
                }
            }
        }

        private async Task<ProcessingCheckpoint> GetOrCreateCheckpointAsync(CancellationToken ct)
        {
            var checkpoint = await _db.ProcessingCheckpoints
                .SingleOrDefaultAsync(x => x.Name == ProcessName, ct);

            if (checkpoint is null)
            {
                checkpoint = new ProcessingCheckpoint
                {
                    Name = ProcessName,
                    LastProcessedChangeVersion = 0L
                };

                _db.ProcessingCheckpoints.Add(checkpoint);
            }

            return checkpoint;
        }

        private async Task<List<ChangedPolicyRow>> GetChangedPoliciesAsync(long lastProcessedChangeVersion, CancellationToken ct)
        {
            return await (
                from p in _db.Policies.AsNoTracking()
                join b in _db.Buildings.AsNoTracking() on p.BuildingId equals b.Id
                join br in _db.Brokers.AsNoTracking() on p.BrokerId equals br.Id
                where p.ChangeVersion > lastProcessedChangeVersion
                orderby p.ChangeVersion
                select new ChangedPolicyRow
                {
                    Id = p.Id,
                    StartDate = p.StartDate,
                    PolicyStatus = p.PolicyStatus,
                    CurrencyId = p.CurrencyId,
                    FinalPremium = p.FinalPremium,
                    FinalPremiumInBaseCurrency = p.FinalPremiumInBaseCurrency,
                    BrokerId = p.BrokerId,
                    BrokerCode = br.BrokerCode,
                    CityId = b.CityId,
                    BuildingType = b.Type,
                    UpdatedAtUtc = p.UpdatedAt,
                    ChangeVersion = p.ChangeVersion
                }
            )
            .Take(BatchSize)
            .ToListAsync(ct);
        }

        private async Task<(int inserts, int updates)> UpsertPolicyReportFactsAsync(List<ChangedPolicyRow> changed, CancellationToken ct)
        {
            var policyIds = changed.Select(x => x.Id).ToList();

            var existingFacts = await _db.PolicyReportFacts
                .Where(f => policyIds.Contains(f.PolicyId))
                .ToDictionaryAsync(f => f.PolicyId, ct);

            int inserts = 0, updates = 0;

            foreach (var x in changed)
            {
                if (existingFacts.TryGetValue(x.Id, out var fact))
                {
                    fact.StartDate = x.StartDate;
                    fact.PolicyStatus = x.PolicyStatus;
                    fact.CurrencyId = x.CurrencyId;
                    fact.FinalPremium = x.FinalPremium;
                    fact.FinalPremiumInBaseCurrency = x.FinalPremiumInBaseCurrency;
                    fact.BrokerId = x.BrokerId;
                    fact.BrokerCode = x.BrokerCode;
                    fact.CityId = x.CityId;
                    fact.BuildingType = x.BuildingType;
                    fact.SourceLastUpdatedUtc = x.UpdatedAtUtc;

                    updates++;
                }
                else
                {
                    _db.PolicyReportFacts.Add(new PolicyReportFact
                    {
                        PolicyId = x.Id,
                        StartDate = x.StartDate,
                        PolicyStatus = x.PolicyStatus,
                        CurrencyId = x.CurrencyId,
                        FinalPremium = x.FinalPremium,
                        FinalPremiumInBaseCurrency = x.FinalPremiumInBaseCurrency,
                        BrokerId = x.BrokerId,
                        BrokerCode = x.BrokerCode,
                        CityId = x.CityId,
                        BuildingType = x.BuildingType,
                        SourceLastUpdatedUtc = x.UpdatedAtUtc
                    });

                    inserts++;
                }
            }

            return (inserts, updates);
        }

        private sealed class ChangedPolicyRow
        {
            public Guid Id { get; init; }
            public DateTime StartDate { get; init; }
            public PolicyStatus PolicyStatus { get; init; }
            public Guid CurrencyId { get; init; }
            public decimal FinalPremium { get; init; }
            public decimal FinalPremiumInBaseCurrency { get; init; }
            public Guid BrokerId { get; init; }
            public string BrokerCode { get; init; } = string.Empty;
            public Guid CityId { get; init; }
            public BuildingType BuildingType { get; init; }
            public DateTime UpdatedAtUtc { get; init; }
            public long ChangeVersion { get; init; }
        }
    }
}