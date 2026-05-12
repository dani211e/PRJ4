using Microsoft.AspNetCore.Identity;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.DB
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<ApiRole> roleManager)
        {
            var roles = typeof(Roles).GetFields().Select(f => f.Name).ToArray();

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApiRole { Name = role });
                }
            }
        }
    }
}