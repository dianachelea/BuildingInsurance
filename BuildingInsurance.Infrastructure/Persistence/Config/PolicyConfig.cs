using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Entities.Management;
using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Entities.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class PolicyConfig : IEntityTypeConfiguration<Policy>
    {
        public void Configure(EntityTypeBuilder<Policy> builder)
        {
            builder.ToTable("Policies");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.PolicyNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(p => p.PolicyNumber)
                .IsUnique();

            builder.Property(p => p.ClientId).IsRequired();

            builder.Property(p => p.BuildingId).IsRequired();

            builder.Property(p => p.BrokerId).IsRequired();

            builder.Property(p => p.CurrencyId).IsRequired();

            builder.Property(p => p.PolicyStatus)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.StartDate)
                .IsRequired();

            builder.Property(p => p.EndDate)
                .IsRequired();

            builder.Property(p => p.BasePremium)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(p => p.FinalPremium)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(p => p.FinalPremiumInBaseCurrency)
                .HasPrecision(12, 2)
                .IsRequired();

            builder.HasOne<Client>()
                .WithMany()
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Building>()
                .WithMany()
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Broker>()
                .WithMany()
                .HasForeignKey(p => p.BrokerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Currency>()
                .WithMany()
                .HasForeignKey(p => p.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.AppliedFees)
                .WithOne()
                .HasForeignKey(paf => paf.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(p => p.AppliedFees)
                .HasField("_appliedFees")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(p => p.AppliedRiskFactors)
                .WithOne()
                .HasForeignKey(parf => parf.PolicyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(p => p.AppliedRiskFactors)
                .HasField("_appliedRiskFactors")
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Property(p => p.CancellationEffectiveDate)
                .IsRequired(false);

            builder.Property(p => p.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            builder.HasIndex(p => new { p.ClientId, p.PolicyStatus });
            builder.HasIndex(p => new { p.BrokerId, p.PolicyStatus });
            builder.HasIndex(p => p.BuildingId);
            builder.HasIndex(p => new { p.CurrencyId, p.PolicyStatus, p.StartDate });
            builder.HasIndex(p => new { p.UpdatedAt, p.Id });
        }
    }
}