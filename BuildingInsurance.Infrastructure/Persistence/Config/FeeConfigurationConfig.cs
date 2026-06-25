using BuildingInsurance.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class FeeConfigurationConfig : IEntityTypeConfiguration<FeeConfiguration>
    {
        public void Configure(EntityTypeBuilder<FeeConfiguration> builder)
        {
            builder.ToTable("FeeConfigurations");

            builder.HasKey(fc => fc.Id);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.FeeType)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(f => f.RiskIndicators)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(f => f.FeePercentage)
                .HasPrecision(5, 4)
                .IsRequired();

            builder.Property(f => f.EffectiveFrom)
                .IsRequired();

            builder.Property(f => f.EffectiveTo)
                .IsRequired();

            builder.Property(f => f.IsActive)
                .IsRequired();

            builder.HasIndex(f => new { f.FeeType, f.RiskIndicators, f.EffectiveFrom, f.EffectiveTo })
                .IsUnique();
        }
    }
}