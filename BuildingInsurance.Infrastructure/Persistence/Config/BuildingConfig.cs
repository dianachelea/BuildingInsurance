using BuildingInsurance.Domain.Entities.Buildings;
using BuildingInsurance.Domain.Entities.Clients;
using BuildingInsurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public class BuildingConfig : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            builder.ToTable("Buildings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.ClientId).IsRequired();
            builder.Property(b => b.CityId).IsRequired();

            builder.HasOne<City>()
                .WithMany()
                .HasForeignKey(b=>b.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Client>()
                .WithMany()
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.Type)
                .HasConversion<string>()
                .IsRequired();
            builder.Property(b => b.RiskIndicators)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(b => b.ConstructionYear).IsRequired();
            builder.Property(b => b.NumberOfFloors).IsRequired();

            builder.Property(b => b.SurfaceArea)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            builder.Property(b => b.InsuredValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.OwnsOne(b => b.Address, a =>
            {
                a.Property(p => p.Street)
                    .HasColumnName("AddressStreet")
                    .HasMaxLength(200)
                    .IsRequired();

                a.Property(p => p.Number)
                    .HasColumnName("AddressNumber")
                    .HasMaxLength(20)
                    .IsRequired();

                a.WithOwner();
            });

            builder.Navigation(b => b.Address).IsRequired();
            builder.HasIndex(b => b.CityId);
            builder.HasIndex(b => new { b.Type, b.CityId });
        }
    }
}