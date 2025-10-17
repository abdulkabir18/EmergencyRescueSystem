using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class NotificationEntityTypeConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.CreatedBy).HasMaxLength(100);
            builder.Property(n => n.CreatedAt).IsRequired();
            builder.Property(n => n.UpdatedAt);
            builder.Property(n => n.DeletedAt);
            builder.Property(n => n.IsDeleted).IsRequired();
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.Type).HasConversion<int>().IsRequired();
            builder.Property(n => n.IsRead).IsRequired();
            builder.Property(n => n.TargetType).HasMaxLength(100);

            builder.HasOne(n => n.Recipient)
                .WithMany(r => r.Notifications)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
