using API.DTO;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class UserRolesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserRolesController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("users-with-roles")]
        public async Task<IActionResult> GetUsersWithRolesAsync()
        {
            LogCurrentUser(nameof(GetUsersWithRolesAsync));
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var result = new List<UserWithRolesDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserWithRolesDto
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return Ok(new ApiResponse<IEnumerable<UserWithRolesDto>>
            {
                Success = true,
                Message = "Users with roles loaded",
                Data = result
            });
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserRolesAsync(string userId)
        {
            LogCurrentUser(nameof(GetUserRolesAsync));
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new ApiResponse<UserWithRolesDto>
            {
                Success = true,
                Message = "User roles loaded",
                Data = new UserWithRolesDto
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleAsync([FromBody] AssignRoleDto dto)
        {
            LogCurrentUser(nameof(AssignRoleAsync));
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            var role = await FindRoleAsync(dto.RoleIdOrName);
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            if (await _userManager.IsInRoleAsync(user, role.Name))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User already has this role",
                    ErrorCode = "ALREADY_IN_ROLE"
                });
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to assign role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role assigned"
            });
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRoleAsync([FromBody] RemoveRoleDto dto)
        {
            LogCurrentUser(nameof(RemoveRoleAsync));
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            var role = await FindRoleAsync(dto.RoleIdOrName);
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            if (!await _userManager.IsInRoleAsync(user, role.Name))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User is not in this role",
                    ErrorCode = "NOT_IN_ROLE"
                });
            }

            var result = await _userManager.RemoveFromRoleAsync(user, role.Name);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to remove role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role removed"
            });
        }

        [HttpPost("set-role")]
        public async Task<IActionResult> SetUserRoleAsync([FromBody] SetUserRoleDto dto)
        {
            LogCurrentUser(nameof(SetUserRoleAsync));

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = "USER_NOT_FOUND"
                });
            }

            var role = await _roleManager.FindByNameAsync(dto.RoleName);
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            // Remove all existing roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(new ApiResponse<IEnumerable<string>>
                    {
                        Success = false,
                        Message = "Failed to remove existing roles",
                        ErrorCode = "IDENTITY_ERROR",
                        Data = removeResult.Errors.Select(e => e.Description)
                    });
                }
            }

            // Assign the new single role
            var addResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!addResult.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to assign role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = addResult.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Role '{role.Name}' assigned successfully"
            });
        }

        private async Task<IdentityRole?> FindRoleAsync(string roleIdOrName)
        {
            if (string.IsNullOrWhiteSpace(roleIdOrName))
            {
                return null;
            }

            var role = await _roleManager.FindByIdAsync(roleIdOrName);
            if (role != null)
            {
                return role;
            }

            role = await _roleManager.FindByNameAsync(roleIdOrName);
            if (role != null)
            {
                return role;
            }

            var normalized = _roleManager.NormalizeKey(roleIdOrName);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized);
            }

            return role;
        }

        private void LogCurrentUser(string action)
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userName = User?.Identity?.Name ?? "anonymous";
            var roles = User?.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToArray() ?? Array.Empty<string>();

            _logger.LogInformation("UserRolesController.{Action} invoked by {UserId}/{UserName} with roles [{Roles}]", action, userId, userName, string.Join(", ", roles));
        }
    }
}
