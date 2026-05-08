using Microsoft.AspNetCore.Identity;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(UserManager<ApiUser> userManager)
        {
            var email = Environment.GetEnvironmentVariable("SEED_ADMIN_EMAIL")
                        ?? throw new InvalidOperationException("SEED_ADMIN_EMAIL is not set in .env");
            var password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD")
                           ?? throw new InvalidOperationException("SEED_ADMIN_PASSWORD is not set in .env");

            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var admin = new ApiUser { UserName = "Admin", Email = email };
            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
                throw new Exception($"Failed to seed admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(admin, Roles.Admin);
        }
    }
}