using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class IncidentMediaEntityTypeConfiguration : IEntityTypeConfiguration<IncidentMedia>
    {
        public void Configure(EntityTypeBuilder<IncidentMedia> builder)
        {
            builder.ToTable("IncidentMedias");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CreatedBy).HasMaxLength(100);
            builder.Property(m => m.CreatedAt).IsRequired();
            builder.Property(m => m.UpdatedAt);
            builder.Property(m => m.DeletedAt);
            builder.Property(m => m.IsDeleted).IsRequired();
            builder.Property(m => m.FileUrl).IsRequired().HasMaxLength(500);
            builder.Property(m => m.MediaType).HasConversion<int>().IsRequired();

            builder.HasOne(m => m.Incident)
                   .WithMany(i => i.Medias)
                   .HasForeignKey(m => m.IncidentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(m => new { m.IncidentId, m.FileUrl });
        }
    }
}
