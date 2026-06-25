namespace BuildingInsurance.Application.Abstractions.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IBuildingRepository Buildings { get; }
        ICountryRepository Countries { get; }
        ICountyRepository Counties { get; }
        ICityRepository Cities { get; }
        IPolicyRepository Policies { get; }
        IBrokerRepository Brokers { get; }
        ICurrencyRepository Currencies { get; }
        IRiskFactorConfigurationRepository RiskFactorConfigurations { get; }
        IFeeConfigurationRepository FeeConfigurations { get; }

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}