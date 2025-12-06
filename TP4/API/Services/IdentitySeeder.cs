using API.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace API.Services
{
    public class IdentitySeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentitySeeder> _logger;
        private readonly IConfiguration _configuration;

        public IdentitySeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<IdentitySeeder> logger,
            IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SeedAsync()
        {
            var superAdminRoleName = _configuration["Identity:SuperAdmin:RoleName"] ?? "SuperAdmin";
            var role = await EnsureRoleAsync(superAdminRoleName);
            var user = await EnsureUserAsync(superAdminRoleName);

            if (role != null && user != null && !await _userManager.IsInRoleAsync(user, superAdminRoleName))
            {
                var result = await _userManager.AddToRoleAsync(user, superAdminRoleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Seeded user {UserEmail} into role {Role}", user.Email, superAdminRoleName);
                }
                else
                {
                    _logger.LogError("Failed to assign user {UserEmail} to role {Role}: {Errors}", user.Email, superAdminRoleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            // Also ensure existing user "Sa3id" is added to SuperAdmin role properly via Identity
            await EnsureUserInRoleAsync("Sa3id", superAdminRoleName);
        }

        private async Task EnsureUserInRoleAsync(string username, string roleName)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found, skipping role assignment", username);
                return;
            }

            _logger.LogInformation("Found user {Username} (Id: {UserId}), checking role {Role}", username, user.Id, roleName);

            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                _logger.LogInformation("User {Username} is already in role {Role}", username, roleName);
                return;
            }

            _logger.LogInformation("Adding user {Username} to role {Role}...", username, roleName);
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully added user {Username} to role {Role}", username, roleName);
            }
            else
            {
                _logger.LogError("Failed to add user {Username} to role {Role}: {Errors}", 
                    username, roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        private async Task<IdentityRole?> EnsureRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                return role;
            }

            var createResult = await _roleManager.CreateAsync(new IdentityRole
            {
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant()
            });

            if (createResult.Succeeded)
            {
                _logger.LogInformation("Seeded role {Role}", roleName);
                return await _roleManager.FindByNameAsync(roleName);
            }

            _logger.LogError("Failed to create role {Role}: {Errors}", roleName, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return null;
        }

        private async Task<ApplicationUser?> EnsureUserAsync(string roleName)
        {
            var email = _configuration["Identity:SuperAdmin:Email"] ?? "superadmin@example.com";
            var password = _configuration["Identity:SuperAdmin:Password"] ?? "SuperAdmin!123";
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return user;
            }

            var newUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                FullName = "Super Admin"
            };

            var createResult = await _userManager.CreateAsync(newUser, password);
            if (createResult.Succeeded)
            {
                _logger.LogInformation("Seeded SuperAdmin user {Email}", email);
                return await _userManager.FindByEmailAsync(email);
            }

            _logger.LogError("Failed to create SuperAdmin user {Email}: {Errors}", email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
            return null;
        }
    }
}
