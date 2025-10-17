using Domain.Entities;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.EntityTypeConfigurations
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);
            builder.Property(u => u.CreatedBy).HasMaxLength(100);
            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(u => u.DeletedAt);
            builder.Property(u => u.UpdatedAt);
            builder.Property(u => u.IsDeleted);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(250);
            builder.Property(u => u.PasswordHash).IsRequired(false);
            builder.Property(u => u.ProfilePictureUrl).IsRequired(false).HasMaxLength(555);
            builder.Property(u => u.Role).HasConversion<int>().IsRequired();
            builder.Property(u => u.IsActive).IsRequired();

            builder.Property(u => u.Email)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(200)
                .HasConversion(
                    v => v.Value,
                    v => new Email(v)
                );

            builder.OwnsOne(u => u.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.State).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
            });

            //builder.OwnsMany(u => u.EmergencyContacts, ec =>
            //{
            //    ec.ToTable("UserEmergencyContacts");
            //    ec.WithOwner().HasForeignKey("UserId");
            //    ec.Property(e => e.Name).IsRequired().HasMaxLength(100);
            //    ec.Property(e => e.IsEmailVerified).IsRequired();
            //    ec.Property(e => e.IsPhoneNumberVerified).IsRequired();

            //    ec.OwnsOne(e => e.PhoneNumber, pn =>
            //    {
            //        pn.Property(p => p.Value).HasColumnName("EmergencyPhoneNumber").HasMaxLength(18);
            //    });

            //    ec.OwnsOne(e => e.Email, email =>
            //    {
            //        email.Property(e => e.Value).HasColumnName("EmergencyEmail").HasMaxLength(200).IsRequired();
            //    });

            //    ec.Property(e => e.Relationship).IsRequired();
            //    ec.Property(e => e.OtherRelationship).HasMaxLength(80);
            //});

            //builder.HasMany(u => u.ChatParticipations)
            //    .WithOne(c => c.User)
            //    .HasForeignKey(u => u.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(u => u.Messages)
            //    .WithOne(m => m.Sender)
            //    .HasForeignKey(s => s.SenderId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Agency)
                .WithOne(a => a.AgencyAdmin)
                .HasForeignKey<Agency>(u => u.AgencyAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Responder)
                .WithOne(r => r.User)
                .HasForeignKey<Responder>(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(u => u.Incidents)
            //    .WithOne(i => i.User)
            //    .HasForeignKey(i => i.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //builder.HasMany(u => u.LiveStreamParticipations)
            //.WithOne(l => l.User)
            //.HasForeignKey(u => u.UserId)
            //.OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Notifications)
                .WithOne(n => n.Recipient)
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}