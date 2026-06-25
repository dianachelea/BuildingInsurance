using BuildingInsurance.Infrastructure.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    public sealed class ReportJobResultConfig : IEntityTypeConfiguration<ReportJobResult>
    {
        public void Configure(EntityTypeBuilder<ReportJobResult> builder)
        {
            builder.ToTable("ReportJobResults");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RowsJson)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => x.JobId)
                .IsUnique();
        }
    }
}