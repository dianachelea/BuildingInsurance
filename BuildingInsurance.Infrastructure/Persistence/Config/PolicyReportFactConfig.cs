using BuildingInsurance.Infrastructure.Persistence.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class PolicyReportFactConfig : IEntityTypeConfiguration<PolicyReportFact>
    {
        public void Configure(EntityTypeBuilder<PolicyReportFact> builder)
        {
            builder.ToTable("PolicyReportFacts");

            builder.HasKey(x => x.PolicyId);

            builder.Property(x => x.PolicyStatus)
                .HasConversion<string>()
                .IsRequired();
            builder.Property(x => x.BuildingType)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.FinalPremium)
                .HasPrecision(12, 2);
            builder.Property(x => x.FinalPremiumInBaseCurrency)
                .HasPrecision(12, 2);

            builder.Property(x => x.BrokerCode)
                .HasMaxLength(12)
                .IsRequired();

            builder.HasIndex(x => new { x.CurrencyId, x.PolicyStatus, x.StartDate });
            builder.HasIndex(x => new { x.BrokerCode, x.CurrencyId, x.PolicyStatus, x.StartDate });
            builder.HasIndex(x => new { x.CityId, x.CurrencyId, x.PolicyStatus, x.StartDate });

            builder.HasIndex(x => x.SourceLastUpdatedUtc);
        }
    }
}