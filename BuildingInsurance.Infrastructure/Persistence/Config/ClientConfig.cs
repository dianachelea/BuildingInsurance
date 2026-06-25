using BuildingInsurance.Domain.Entities.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public class ClientConfig : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Type)
                .HasConversion<string>()
                .IsRequired();
            builder.Property(c => c.FullName)
                .IsRequired().HasMaxLength(200);
            builder.Property(c => c.PersonalIdentificationNumber)
                .HasMaxLength(20);
            builder.Property(c => c.CompanyRegistrationNumber)
                .HasMaxLength(20);

            builder.HasIndex(c => c.PersonalIdentificationNumber)
                .IsUnique()
                .HasFilter("[PersonalIdentificationNumber] IS NOT NULL");
            builder.HasIndex(c => c.CompanyRegistrationNumber)
                .IsUnique()
                .HasFilter("[CompanyRegistrationNumber] IS NOT NULL");

            builder.OwnsOne(c => c.ContactInfo, ci =>
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

                ci.OwnsOne(p => p.Address, a =>
                {
                    a.Property(x => x.Street)
                        .HasColumnName("AddressStreet")
                        .HasMaxLength(200)
                        .IsRequired(false);

                    a.Property(x => x.Number)
                        .HasColumnName("AddressNumber")
                        .HasMaxLength(20)
                        .IsRequired(false);
                });
            });

            builder.Navigation(x => x.ContactInfo)
                .IsRequired();
        }
    }
}