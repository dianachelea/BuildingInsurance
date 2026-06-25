using BuildingInsurance.Domain.Entities;
using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Entities.Geography;
using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Entities.Outbox;
using BuildingInsurance.Domain.Entities.Policies;
using BuildingInsurance.Infrastructure.Persistence.Cursors;
using BuildingInsurance.Infrastructure.Persistence.Reporting;
using BuildingInsurance.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;

namespace BuildingInsurance.Infrastructure.Persistence
{
    public class BuildingInsuranceDbContext : DbContext
    {
        public BuildingInsuranceDbContext(DbContextOptions<BuildingInsuranceDbContext> options) : base(options) { }
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<County> Counties => Set<County>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Broker> Brokers => Set<Broker>();
        public DbSet<Administrator> Administrators => Set<Administrator>();
        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<FeeConfiguration> FeeConfigurations => Set<FeeConfiguration>();
        public DbSet<RiskFactorConfiguration> RiskFactorConfigurations => Set<RiskFactorConfiguration>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<PolicyAppliedFee> PolicyAppliedFees => Set<PolicyAppliedFee>();
        public DbSet<PolicyAppliedRiskFactor> PolicyAppliedRiskFactors => Set<PolicyAppliedRiskFactor>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
        public DbSet<PolicyReportFact> PolicyReportFacts => Set<PolicyReportFact>();
        public DbSet<ProcessingCheckpoint> ProcessingCheckpoints => Set<ProcessingCheckpoint>();
        public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
        public DbSet<ReportJobResult> ReportJobResults => Set<ReportJobResult>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildingInsuranceDbContext).Assembly);

            var isSqlServer = Database.IsSqlServer();

            modelBuilder.Entity<Policy>(b =>
            {
                if (isSqlServer)
                {
                    b.Property(p => p.RowVersion)
                        .IsRowVersion()
                        .IsConcurrencyToken();

                    b.Property(p => p.ChangeVersion)
                        .HasComputedColumnSql("CONVERT(bigint, [RowVersion])", stored: true);

                    b.HasIndex(p => p.ChangeVersion);
                }
                else
                {
                    b.Property(p => p.RowVersion)
                        .IsRequired(false)
                        .HasColumnType("BLOB");

                    b.Property(p => p.ChangeVersion)
                        .IsRequired()
                        .HasDefaultValue(0L);

                    b.HasIndex(p => p.ChangeVersion);
                }
            });

            modelBuilder.Entity<OutboxMessage>(b =>
            {
                b.ToTable("OutboxMessages");
                b.HasKey(x => x.Id);

                b.Property(x => x.Type).IsRequired().HasMaxLength(500);
                b.Property(x => x.Payload).IsRequired();
                b.Property(x => x.OccurredOn).IsRequired();
            });

            modelBuilder.Entity<ProcessingCheckpoint>(b =>
            {
                b.ToTable("ProcessingCheckpoint");
                b.HasKey(x => x.Name);

                b.Property(x => x.Name).HasMaxLength(100).IsRequired();
                b.Property(x => x.LastProcessedChangeVersion).IsRequired();
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(AggregateRoot.CreatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = utcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(AggregateRoot.UpdatedAt)).CurrentValue = utcNow;
                    entry.Property(nameof(AggregateRoot.UpdatedAt)).IsModified = true;
                    entry.Property(nameof(AggregateRoot.CreatedAt)).IsModified = false;
                }
            }

            if (!Database.IsSqlServer())
            {
                var nowTicks = DateTime.UtcNow.Ticks;
                var policyEntries = ChangeTracker.Entries<Policy>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

                foreach (var e in policyEntries)
                {
                    e.Property(nameof(Policy.ChangeVersion)).CurrentValue = nowTicks;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}