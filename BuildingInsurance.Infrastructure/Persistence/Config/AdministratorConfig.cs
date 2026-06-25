using BuildingInsurance.Domain.Entities.Management;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class AdministratorConfig : IEntityTypeConfiguration<Administrator>
    {
        public void Configure(EntityTypeBuilder<Administrator> builder)
        {
            builder.ToTable("Administrators");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.OwnsOne(x => x.ContactInfo, ci =>
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

            builder.Navigation(x => x.ContactInfo).IsRequired();

            builder.Property(x => x.AdminRole)
                .HasConversion<string>()
                .IsRequired();
        }
    }
}