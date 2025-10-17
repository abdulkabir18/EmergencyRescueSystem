using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class AgencyEntityTypeConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.ToTable("Agencies");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.CreatedBy).HasMaxLength(100);
            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt);
            builder.Property(a => a.DeletedAt);
            builder.Property(a => a.IsDeleted).IsRequired();
            builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
            builder.Property(a => a.LogoUrl).HasMaxLength(500);

            builder.Property(u => u.Email)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(200)
                .HasConversion(
                    v => v.Value,
                    v => new Email(v)
                );

            builder.Property(u => u.PhoneNumber)
                .HasColumnName("PhoneNumber")
                .IsRequired()
                .HasMaxLength(18)
                .HasConversion(
                    v => v.Value,
                    v => new PhoneNumber(v)
                );

            builder.OwnsOne(a => a.Address, address =>
            {
                address.Property(ad => ad.Street).HasMaxLength(200);
                address.Property(ad => ad.City).HasMaxLength(100);
                address.Property(ad => ad.State).HasMaxLength(100);
                address.Property(ad => ad.PostalCode).HasMaxLength(20);
                address.Property(ad => ad.Country).HasMaxLength(100);
            });

            builder.HasMany(a => a.Responders)
                .WithOne(r => r.Agency)
                .HasForeignKey(u => u.AgencyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.AgencyAdmin)
                .WithOne(a => a.Agency)
                .HasForeignKey<Agency>(a => a.AgencyAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(a => a.SupportedIncidents)
            //    .WithOne(s => s.Agency)
            //    .HasForeignKey(si => si.AgencyId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //builder.HasMany(a => a.SupportedWorkTypes)
            //    .WithOne(s => s.Agency)
            //    .HasForeignKey(sw => sw.AgencyId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
