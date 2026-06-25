using BuildingInsurance.Domain.Entities.Metadata;
using BuildingInsurance.Domain.Entities.Policies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class PolicyAppliedFeeConfig : IEntityTypeConfiguration<PolicyAppliedFee>
    {
        public void Configure(EntityTypeBuilder<PolicyAppliedFee> builder)
        {
            builder.ToTable("PolicyAppliedFees");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PolicyId)
                .IsRequired();

            builder.Property(x => x.FeeConfigurationId)
                .IsRequired();

            builder.Property(x => x.FeeName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Percentage)
                .HasPrecision(5, 4)
                .IsRequired();

            builder.Property(x => x.AppliedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.PolicyId);
            builder.HasIndex(x => x.FeeConfigurationId);

            builder.HasIndex(x => new { x.PolicyId, x.FeeConfigurationId })
                .IsUnique();

            builder.HasOne<FeeConfiguration>()
                .WithMany()
                .HasForeignKey(x => x.FeeConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}