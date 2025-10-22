using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
       public class IncidentResponderEntityTypeConfiguration : IEntityTypeConfiguration<IncidentResponder>
       {
              public void Configure(EntityTypeBuilder<IncidentResponder> builder)
              {
                     builder.ToTable("IncidentResponders");

                     builder.HasKey(ir => ir.Id);

                     builder.Property(ir => ir.CreatedBy).HasMaxLength(100);
                     builder.Property(ir => ir.CreatedAt).IsRequired();
                     builder.Property(ir => ir.UpdatedAt);
                     builder.Property(ir => ir.DeletedAt);
                     builder.Property(ir => ir.IsDeleted).IsRequired();
                     builder.Property(ir => ir.Role).HasConversion<int>().IsRequired();
                     builder.Property(ir => ir.IsActive).IsRequired();

                     builder.HasOne(ir => ir.Incident)
                            .WithMany(i => i.AssignedResponders)
                            .HasForeignKey(ir => ir.IncidentId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasOne(ir => ir.Responder)
                            .WithMany(r => r.IncidentAssignments)
                            .HasForeignKey(ir => ir.ResponderId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasIndex(ir => new { ir.IncidentId, ir.ResponderId }).IsUnique();
              }
       }
}
