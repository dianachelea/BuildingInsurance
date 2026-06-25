using BuildingInsurance.Infrastructure.Persistence.Cursors;
using BuildingInsurance.Infrastructure.Persistence.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingInsurance.Infrastructure.Persistence.Config
{
    internal class ProcessingCheckpointConfig : IEntityTypeConfiguration<ProcessingCheckpoint>
    {
        public void Configure(EntityTypeBuilder<ProcessingCheckpoint> builder)
        {
            builder.ToTable("ProcessingCheckpoint");

            builder.HasKey(x => x.Name);

            builder.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();
            
            builder.Property(x => x.LastProcessedChangeVersion)
                .IsRequired();
        }
    }
}