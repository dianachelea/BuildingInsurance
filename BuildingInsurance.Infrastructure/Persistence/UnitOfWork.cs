using BuildingInsurance.Application.Abstractions.Persistence;
using BuildingInsurance.Domain.Entities;
using BuildingInsurance.Domain.Entities.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingInsurance.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BuildingInsuranceDbContext _buildingInsuranceDbContext;
        private IDbContextTransaction? _dbContextTransaction;
        public IClientRepository Clients { get; }

        public IBuildingRepository Buildings { get; }

        public ICountryRepository Countries { get; }

        public ICountyRepository Counties { get; }
        public ICityRepository Cities { get; }

        public IPolicyRepository Policies { get; }

        public IBrokerRepository Brokers { get; }

        public ICurrencyRepository Currencies { get; }

        public IRiskFactorConfigurationRepository RiskFactorConfigurations { get; }

        public IFeeConfigurationRepository FeeConfigurations { get; }

        public UnitOfWork(BuildingInsuranceDbContext buildingInsuranceDbContext, IClientRepository clients, IBuildingRepository buildings, ICityRepository cities, ICountyRepository counties, ICountryRepository countries,
            IPolicyRepository policies, IBrokerRepository brokers, ICurrencyRepository currencies, IRiskFactorConfigurationRepository riskFactorConfigurations, IFeeConfigurationRepository feeConfigurations)
        {
            _buildingInsuranceDbContext = buildingInsuranceDbContext;
            Clients = clients;
            Buildings = buildings;
            Cities = cities;
            Counties = counties;
            Countries = countries;
            Policies = policies;
            Brokers = brokers;
            Currencies = currencies;
            RiskFactorConfigurations = riskFactorConfigurations;
            FeeConfigurations = feeConfigurations;
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _dbContextTransaction = await _buildingInsuranceDbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            var aggregatesWithEvents = await HandleOutboxMessagesAsync(cancellationToken);

            try
            {
				await _buildingInsuranceDbContext.SaveChangesAsync(cancellationToken);

				DisposeDomainEvents(aggregatesWithEvents);

				if (_dbContextTransaction != null)
				{
					await _dbContextTransaction.CommitAsync(cancellationToken);
					await _dbContextTransaction.DisposeAsync();
					_dbContextTransaction = null;
				}
			}
            catch
            {
				var trackedOutbox = _buildingInsuranceDbContext.ChangeTracker
                    .Entries<OutboxMessage>()
		            .Where(e => e.State == EntityState.Added)
		            .ToList();

				foreach (var entry in trackedOutbox)
					entry.State = EntityState.Detached;

				throw;
			}
        }

        public void Dispose()
        {
            _dbContextTransaction?.Dispose();
            _buildingInsuranceDbContext.Dispose();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_dbContextTransaction != null)
            {
                await _dbContextTransaction.RollbackAsync(cancellationToken);
                await _dbContextTransaction.DisposeAsync();
                _dbContextTransaction = null;
            }
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _buildingInsuranceDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<AggregateRoot>> HandleOutboxMessagesAsync(CancellationToken cancellationToken = default)
        {
            var aggregatesWithEvents = _buildingInsuranceDbContext.ChangeTracker
                .Entries<AggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Count != 0)
                .ToList();

            var domainEvents = aggregatesWithEvents
                .SelectMany(e => e.Entity.DomainEvents)
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                var outboxMessage = new OutboxMessage(domainEvent);
                await _buildingInsuranceDbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
            }
            return aggregatesWithEvents.Select(e => e.Entity).ToList();
        }

        public static void DisposeDomainEvents(List<AggregateRoot> aggregatesWithEvents)
        {
            foreach (var entry in aggregatesWithEvents)
            {
                entry.ClearDomainEvents();
            }
        }
    }
}