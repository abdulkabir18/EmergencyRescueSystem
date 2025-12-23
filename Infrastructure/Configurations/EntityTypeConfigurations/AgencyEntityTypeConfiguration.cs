using Domain.Entities;
using Domain.Enums;
using Domain.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                address.Property(ad => ad.LGA).HasMaxLength(100);
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

            var jsonOptions = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            builder.Property(a => a.SupportedIncidents)
                .HasColumnName("SupportedIncidents")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => string.IsNullOrEmpty(v)
                        ? new List<IncidentType>()
                        : JsonSerializer.Deserialize<List<IncidentType>>(v, jsonOptions)!
                )
                .Metadata.SetValueComparer(new ValueComparer<ICollection<IncidentType>>(
                    (c1, c2) => ReferenceEquals(c1, c2) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => (ICollection<IncidentType>)(c == null ? new List<IncidentType>() : c.ToList())
                ));
        }
    }
}