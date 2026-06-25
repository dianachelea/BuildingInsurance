using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Entities.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class PolicyAppliedRiskFactorConfig : IEntityTypeConfiguration<PolicyAppliedRiskFactor>
    {
        public void Configure(EntityTypeBuilder<PolicyAppliedRiskFactor> builder)
        {
            builder.ToTable("PolicyAppliedRiskFactors");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PolicyId).IsRequired();
            builder.Property(x => x.RiskFactorConfigurationId).IsRequired();

            builder.Property(x => x.Level)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(x => x.ReferenceId);

            builder.Property(x => x.BuildingType)
                .HasConversion<string?>();

            builder.Property(x => x.AdjustmentPercentage)
                .HasPrecision(5, 4)
                .IsRequired();

            builder.Property(x => x.AppliedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.PolicyId, x.RiskFactorConfigurationId })
                .IsUnique();

            builder.HasOne<RiskFactorConfiguration>()
                .WithMany()
                .HasForeignKey(x => x.RiskFactorConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PolicyId);
            builder.HasIndex(x => x.RiskFactorConfigurationId);
        }
    }
}