using Domain.Common.Security;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObject;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeding
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ProjectDbContext context, IPasswordHasher passwordHasher)
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin))
            {
                var superAdmin = new User(
                    fullName: "Naija Resuce Admin",
                    email: new Email("abdulkabirfagbohun@gmail.com"),
                    gender: Gender.Male,
                    role: UserRole.SuperAdmin
                );

                superAdmin.SetPassword($"Admin@12345{superAdmin.Id}", passwordHasher);

                await context.Users.AddAsync(superAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}