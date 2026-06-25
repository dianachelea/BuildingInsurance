using BuildingInsurance.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class RiskFactorConfigurationConfig : IEntityTypeConfiguration<RiskFactorConfiguration>
    {
        public void Configure(EntityTypeBuilder<RiskFactorConfiguration> builder)
        {
            builder.ToTable("RiskFactorConfigurations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Level)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(r => r.ReferenceId);

            builder.Property(r => r.BuildingType)
                .HasConversion<string?>();

            builder.Property(r => r.AdjustmentPercentage)
                .HasPrecision(5, 4)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .IsRequired();

            builder.HasIndex(r => new { r.Level, r.ReferenceId })
                .IsUnique();

            builder.HasIndex(r => new { r.Level, r.BuildingType })
                .IsUnique();
        }
    }
}