using Microsoft.AspNetCore.Identity;
using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.DB
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<ApiRole> roleManager)
        {
            foreach (var role in new[] { Roles.Admin, Roles.Player })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApiRole { Name = role });
                }
            }
        }
    }
}