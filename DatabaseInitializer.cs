using Microsoft.EntityFrameworkCore;
using MyApi.Entities;

namespace MyApi
{
    public static class DatabaseInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            // --- 1. Seed Roles ---
            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { Id = 1, Name = "Admin" },
                    new Role { Id = 2, Name = "User" }
                );

                await db.SaveChangesAsync();
            }

            // --- 2. Seed Admin User ---
            if (!await db.Users.AnyAsync(u => u.UserName == "admin"))
            {
                var adminRole = await db.Roles.FirstAsync(r => r.Name == "Admin");

                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    RoleId = adminRole.Id
                };

                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }
        }
    }
}
