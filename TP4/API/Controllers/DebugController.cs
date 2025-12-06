using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _context;

        public DebugController(
            UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ApplicationContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        /// <summary>
        /// Debug endpoint to check user roles directly from database
        /// </summary>
        [HttpGet("user-roles/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserRolesDebug(string username)
        {
            Console.WriteLine($"=== [DebugController] Checking roles for user: {username} ===");
            
            // Find user
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return NotFound(new { message = $"User '{username}' not found" });
            }

            Console.WriteLine($"[DebugController] Found user: Id={user.Id}, UserName={user.UserName}");

            // Get roles via UserManager
            var rolesViaManager = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"[DebugController] Roles via UserManager: {string.Join(", ", rolesViaManager)}");

            // Get roles directly from database
            var userRolesFromDb = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync();
            Console.WriteLine($"[DebugController] UserRoles from DB: {userRolesFromDb.Count} entries");

            var roleDetailsFromDb = new List<object>();
            foreach (var ur in userRolesFromDb)
            {
                var role = await _roleManager.FindByIdAsync(ur.RoleId);
                roleDetailsFromDb.Add(new
                {
                    UserRoleUserId = ur.UserId,
                    UserRoleRoleId = ur.RoleId,
                    RoleName = role?.Name,
                    RoleNormalizedName = role?.NormalizedName
                });
                Console.WriteLine($"  - RoleId: {ur.RoleId} -> RoleName: {role?.Name}");
            }

            // Get all roles in system
            var allRoles = await _roleManager.Roles.ToListAsync();
            Console.WriteLine($"[DebugController] All roles in system:");
            foreach (var r in allRoles)
            {
                Console.WriteLine($"  - Id: {r.Id}, Name: {r.Name}, NormalizedName: {r.NormalizedName}");
            }

            return Ok(new
            {
                user = new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    NormalizedUserName = user.NormalizedUserName,
                    Email = user.Email
                },
                rolesViaUserManager = rolesViaManager.ToList(),
                rolesFromDatabase = roleDetailsFromDb,
                allSystemRoles = allRoles.Select(r => new { r.Id, r.Name, r.NormalizedName }).ToList()
            });
        }

        /// <summary>
        /// Get current authenticated user info
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "No user identifier in claims" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userId = user.Id,
                userName = user.UserName,
                email = user.Email,
                roles = roles.ToList(),
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }
    }
}
