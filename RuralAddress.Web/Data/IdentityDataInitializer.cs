using Microsoft.AspNetCore.Identity;
using RuralAddress.Core.Entities;

namespace RuralAddress.Web.Data;

public static class IdentityDataInitializer
{
    public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await roleManager.RoleExistsAsync("Gerente"))
        {
            await roleManager.CreateAsync(new IdentityRole("Gerente"));
        }

        if (!await roleManager.RoleExistsAsync("Pesquisador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Pesquisador"));
        }

        if (!await roleManager.RoleExistsAsync("Basico"))
        {
            await roleManager.CreateAsync(new IdentityRole("Basico"));
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new IdentityRole("User"));
        }

        // Seed Admin User
        if (await userManager.FindByNameAsync("admin@rural.com") == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@rural.com",
                Email = "admin@rural.com",
                EmailConfirmed = true,
                MustChangePassword = false
            };

            var result = await userManager.CreateAsync(user, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
        else
        {
            // Ensure existing admin has the role
            var user = await userManager.FindByNameAsync("admin@rural.com");
            if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
