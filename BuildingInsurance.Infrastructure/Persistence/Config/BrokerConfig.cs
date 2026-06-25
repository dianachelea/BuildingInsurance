using BuildingInsurance.Domain.Entities.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class BrokerConfig :IEntityTypeConfiguration<Broker>
    {
        public void Configure(EntityTypeBuilder<Broker> builder)
        {
            builder.ToTable("Brokers");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BrokerCode)
                .IsRequired()
                .HasMaxLength(12);

            builder.HasIndex(b => b.BrokerCode).IsUnique();

            builder.Property(b => b.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(b => b.ContactInfo, ci =>
            {
                ci.Property(p => p.Email)
                    .HasColumnName("Email")
                    .HasMaxLength(200)
                    .IsRequired();

                ci.HasIndex(p => p.Email).IsUnique();

                ci.Property(p => p.Phone)
                    .HasColumnName("Phone")
                    .HasMaxLength(20)
                    .IsRequired();

                ci.Ignore(p => p.Address);
            });

            builder.Navigation(b => b.ContactInfo).IsRequired();

            builder.Property(b => b.BrokerStatus)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.CommissionPercentage)
                .HasPrecision(5, 4)
                .IsRequired(false);
        }
    }
}