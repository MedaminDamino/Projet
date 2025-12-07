using System.Security.Claims;

namespace API.Helpers;

public static class RoleHelper
{
    /// <summary>
    /// Case-insensitive role check. Checks if the user has the specified role,
    /// regardless of case differences.
    /// </summary>
    public static bool IsInRoleCaseInsensitive(this ClaimsPrincipal? user, string roleName)
    {
        if (user == null || string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        // Check standard IsInRole first (for exact match)
        if (user.IsInRole(roleName))
        {
            return true;
        }

        // Check all role claims case-insensitively
        var roleClaims = user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type.Contains("role", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value);

        return roleClaims.Any(role => 
            string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));
    }
}


