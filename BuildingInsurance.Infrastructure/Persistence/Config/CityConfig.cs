using BuildingInsurance.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public class CityConfig : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.ToTable("Cities");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(c => new { c.CountyId, c.Name })
                .IsUnique();

            builder.HasOne<County>()
                   .WithMany()
                   .HasForeignKey(c => c.CountyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}