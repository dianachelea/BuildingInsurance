using BuildingInsurance.Domain.Common;
using BuildingInsurance.Domain.Constants;
using BuildingInsurance.Domain.Enums;
using BuildingInsurance.Domain.Events;
using BuildingInsurance.Domain.Interfaces;

namespace BuildingInsurance.Domain.Entities.Policies
{
    public class Policy : AggregateRoot, IHasId
    {
        public Guid Id { get; private set; }
        public string PolicyNumber { get; private set; } = null!;
        public Guid ClientId { get; private set; }
        public Guid BuildingId { get; private set; }
        public Guid BrokerId { get; private set; }
        public PolicyStatus PolicyStatus { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal BasePremium { get; private set; }
        public decimal FinalPremium { get; private set; }
        public decimal FinalPremiumInBaseCurrency { get; private set; }
        public Guid CurrencyId { get; private set; }
        public DateTime? CancellationEffectiveDate { get; private set; }
        public byte[] RowVersion { get; private set; } = null!;
        public long ChangeVersion { get; private set; }

        private readonly List<PolicyAppliedFee> _appliedFees = new();
        public IReadOnlyCollection<PolicyAppliedFee> AppliedFees => _appliedFees.AsReadOnly();

        private readonly List<PolicyAppliedRiskFactor> _appliedRiskFactors = new();
        public IReadOnlyCollection<PolicyAppliedRiskFactor> AppliedRiskFactors => _appliedRiskFactors.AsReadOnly();
        private Policy() { }

        public static Policy CreateDraft(Guid clientId, Guid buildingId, Guid brokerId, Guid currencyId, DateTime startDate, DateTime endDate, decimal basePremium)
        {
            ValidateIds(clientId, buildingId, brokerId, currencyId);
            ValidatePeriod(startDate, endDate);

            if (basePremium <= 0)
                throw new ArgumentOutOfRangeException(nameof(basePremium), "Base premium must be positive.");

            return new Policy
            {
                Id = Guid.NewGuid(),
                PolicyNumber = GeneratePolicyNumber(),
                ClientId = clientId,
                BuildingId = buildingId,
                BrokerId = brokerId,
                CurrencyId = currencyId,
                StartDate = startDate,
                EndDate = endDate,
                BasePremium = basePremium,
                FinalPremium = 0m,
                PolicyStatus = PolicyStatus.Draft
            };
        }

        public void Activate(DateTime nowUtc)
        {
            if (StartDate < nowUtc)
                throw new InvalidOperationException("Start date should not be in the past.");

            if (PolicyStatus != PolicyStatus.Draft)
                throw new InvalidOperationException("Only Draft policies can be activated.");

            if (nowUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("nowUtc must be UTC.");
            
            if (FinalPremium <= 0m)
                throw new InvalidOperationException("Policy must be priced before activation.");
            
            if (FinalPremiumInBaseCurrency <= 0m)
                throw new InvalidOperationException("Policy base currency premium must be set before activation.");
            
            PolicyStatus = PolicyStatus.Active;
        }

        public void Cancel(string reasonInput, DateTime cancellationDate)
        {
            if (PolicyStatus != PolicyStatus.Active)
                throw new InvalidOperationException("Only Active policies can be cancelled.");

            if (cancellationDate.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Dates must be UTC.");

            if (cancellationDate < StartDate )
                throw new InvalidOperationException("Cancellation effective date cannot be before StartDate.");

            if (cancellationDate > EndDate)
                throw new InvalidOperationException("Cancellation effective date cannot be after EndDate.");

            if (!ReasonNormalizer.TryNormalize(reasonInput, CancellationReasons.Allowed, out var normalizedReason))
                throw new InvalidOperationException("Invalid cancellation reason.");
            
            CancellationEffectiveDate = cancellationDate;
            PolicyStatus = PolicyStatus.Cancelled;
            AddDomainEvent(new PolicyCancelled(Id, normalizedReason, cancellationDate));
        }

        public void SetPricing(decimal finalPremium, IEnumerable<PolicyAppliedFee> appliedFees, IEnumerable<PolicyAppliedRiskFactor> appliedRiskFactors)
        {
            if (PolicyStatus != PolicyStatus.Draft)
                throw new InvalidOperationException("Pricing can be set only for Draft policies.");

            if (finalPremium <= 0)
                throw new ArgumentOutOfRangeException(nameof(finalPremium));

            _appliedFees.Clear();
            _appliedFees.AddRange(appliedFees);

            _appliedRiskFactors.Clear();
            _appliedRiskFactors.AddRange(appliedRiskFactors);

            FinalPremium = finalPremium;
        }

        private static void ValidatePeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate.Kind != DateTimeKind.Utc || endDate.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Dates must be in UTC.");

            if (endDate <= startDate)
                throw new ArgumentException("EndDate must be after StartDate.");
        }

        private static void ValidateIds(Guid clientId, Guid buildingId, Guid brokerId, Guid currencyId)
        {
            if (clientId == Guid.Empty) 
                throw new ArgumentException("ClientId is required.");
            if (buildingId == Guid.Empty) 
                throw new ArgumentException("BuildingId is required.");
            if (brokerId == Guid.Empty) 
                throw new ArgumentException("BrokerId is required.");
            if (currencyId == Guid.Empty) 
                throw new ArgumentException("CurrencyId is required.");
        }

        private static string GeneratePolicyNumber()
        {
            var suffix = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();
            return $"POL-{suffix}";
        }

        public void SetFinalPremium(decimal finalPremium)
        {
            if (PolicyStatus != PolicyStatus.Draft)
                throw new InvalidOperationException("Final premium can be updated only for Draft policies.");
            if (finalPremium <= 0) 
                throw new ArgumentOutOfRangeException(nameof(finalPremium));
            FinalPremium = finalPremium;
        }

        public void SetFinalPremiumInBaseCurrency(decimal value)
        {
            if (PolicyStatus != PolicyStatus.Draft)
                throw new InvalidOperationException("Base currency premium can be set only for Draft policies.");

            if (value <= 0m)
                throw new ArgumentOutOfRangeException(nameof(value));

            FinalPremiumInBaseCurrency = value;
        }
    }
}