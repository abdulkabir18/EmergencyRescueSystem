using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class ResponderEntityTypeConfiguration : IEntityTypeConfiguration<Responder>
    {
        public void Configure(EntityTypeBuilder<Responder> builder)
        {
            builder.ToTable("Responders");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.CreatedBy).HasMaxLength(100);
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt);
            builder.Property(a => a.DeletedAt);
            builder.Property(a => a.IsDeleted).IsRequired();
            builder.Property(r => r.Status).HasConversion<int>().IsRequired();

            builder.OwnsOne(r => r.Coordinates, loc =>
            {
                loc.Property(l => l.Latitude)
                         .HasColumnName("AssignedLatitude")
                         .HasPrecision(9, 6);

                loc.Property(l => l.Longitude)
                         .HasColumnName("AssignedLongitude")
                         .HasPrecision(9, 6);
            });

            builder.HasOne(r => r.User)
                .WithOne(u => u.Responder)
                .HasForeignKey<Responder>(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Agency)
                .WithMany(a => a.Responders)
                .HasForeignKey(r => r.AgencyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.IncidentAssignments)
                .WithOne(ia => ia.Responder)
                .HasForeignKey(ia => ia.ResponderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
