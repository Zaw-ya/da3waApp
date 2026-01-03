using Da3wa.Domain;
using Da3wa.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Da3wa.Infrastructure.Data
{
    public class ApplicationDbContextSeed
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<ApplicationDbContextSeed> _logger;

        public ApplicationDbContextSeed(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<ApplicationDbContextSeed> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[]
            {
                AppRoles.Administrator,
                AppRoles.Operator,
                AppRoles.Supervisor,
                AppRoles.Accounting,
                AppRoles.Agent,
                AppRoles.Client
            };

            foreach (var roleName in roles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Role '{RoleName}' created successfully", roleName);
                    }
                    else
                    {
                        _logger.LogError("Failed to create role '{RoleName}': {Errors}",
                            roleName,
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogInformation("Role '{RoleName}' already exists", roleName);
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            var adminEmail = "admin@da3wa.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    _logger.LogInformation("Administrator user created successfully");
                    await _userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);
                }
                else
                {
                    _logger.LogError("Failed to create administrator user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                _logger.LogInformation("Administrator user already exists");
                
                // Ensure user is in Administrator role if they exist
                if (!await _userManager.IsInRoleAsync(adminUser, AppRoles.Administrator))
                {
                    await _userManager.AddToRoleAsync(adminUser, AppRoles.Administrator);
                    _logger.LogInformation("Added existing user to Administrator role");
                }
            }
        }
    }
}
