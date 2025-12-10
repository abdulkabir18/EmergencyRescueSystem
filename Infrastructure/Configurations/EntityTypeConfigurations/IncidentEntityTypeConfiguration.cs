using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class IncidentEntityTypeConfiguration : IEntityTypeConfiguration<Incident>
    {
        public void Configure(EntityTypeBuilder<Incident> builder)
        {
            builder.ToTable("Incidents");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.CreatedBy).HasMaxLength(100);
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt);
            builder.Property(a => a.DeletedAt);
            builder.Property(a => a.IsDeleted).IsRequired();

            builder.Property(i => i.Title).IsRequired(false).HasMaxLength(55);
            builder.Property(a => a.Confidence).IsRequired(false);
            builder.Property(i => i.Type).HasConversion<int>().IsRequired();
            builder.Property(i => i.Status).HasConversion<int>().IsRequired();
            builder.Property(i => i.OccurredAt).IsRequired();
            builder.Property(i => i.ReferenceCode).IsRequired().HasMaxLength(50);
            builder.HasIndex(i => i.ReferenceCode).IsUnique();

            builder.OwnsOne(i => i.Coordinates, location =>
            {
                location.Property(l => l.Latitude)
                        .HasColumnName("Latitude")
                        .HasPrecision(9, 6);

                location.Property(l => l.Longitude)
                        .HasColumnName("Longitude")
                        .HasPrecision(9, 6);
            });

            builder.OwnsOne(i => i.Media, medias =>
            {
                medias.Property(m => m.MediaType).IsRequired();
                medias.Property(m => m.FileUrl).IsRequired().HasMaxLength(150);
            });

            builder.OwnsOne(i => i.Address, address =>
            {
                address.Property(ad => ad.Street).HasMaxLength(200).IsRequired(false);
                address.Property(ad => ad.City).HasMaxLength(100).IsRequired(false);
                address.Property(ad => ad.LGA).HasMaxLength(100).IsRequired(false);
                address.Property(ad => ad.State).HasMaxLength(100);
                address.Property(ad => ad.PostalCode).HasMaxLength(20);
                address.Property(ad => ad.Country).HasMaxLength(100);
            });

            builder.HasMany(i => i.AssignedResponders)
                   .WithOne(ar => ar.Incident)
                   .HasForeignKey(ar => ar.IncidentId);


            //builder.HasMany(i => i.AssignedResponders)
            //       .WithMany(ar => ar.IncidentAssignments)
            //       .UsingEntity<Dictionary<string, object>>(
            //            "IncidentResponder",
            //            j => j
            //                .HasOne<Responder>()
            //                .WithMany()
            //                .HasForeignKey("ResponderId")
            //                .OnDelete(DeleteBehavior.Cascade),
            //            j => j
            //                .HasOne<Incident>()
            //                .WithMany()
            //                .HasForeignKey("IncidentId")
            //                .OnDelete(DeleteBehavior.Cascade),
            //            j =>
            //            {
            //                j.HasKey("IncidentId", "ResponderId");
            //                j.ToTable("IncidentResponders");
            //            });

            //builder.HasMany(i => i.Medias)
            //       .WithOne(m => m.Incident)
            //       .HasForeignKey(m => m.IncidentId);
        }
    }

}