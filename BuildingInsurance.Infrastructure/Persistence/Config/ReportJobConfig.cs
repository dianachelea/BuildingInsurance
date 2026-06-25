using BuildingInsurance.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class ReportJobConfig : IEntityTypeConfiguration<ReportJob>
    {
        public void Configure(EntityTypeBuilder<ReportJob> builder)
        {
            builder.ToTable("ReportJobs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Progress)
                .IsRequired();

            builder.Property(x => x.PayloadJson)
                .IsRequired();

            builder.Property(x => x.Error)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });
        }
    }
}