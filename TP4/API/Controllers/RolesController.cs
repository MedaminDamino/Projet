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
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationContext _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(RoleManager<IdentityRole> roleManager, ApplicationContext context, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            LogCurrentUser(nameof(GetAllAsync));
            var roles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => MapRole(r))
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<RoleDto>>
            {
                Success = true,
                Message = "Roles loaded",
                Data = roles
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            LogCurrentUser(nameof(GetByIdAsync));
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role loaded",
                Data = MapRole(role)
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] RoleCreateDto dto)
        {
            LogCurrentUser(nameof(CreateAsync));
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid role payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            if (await _roleManager.RoleExistsAsync(dto.Name))
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role name already exists",
                    ErrorCode = "ROLE_EXISTS"
                });
            }

            var normalized = _roleManager.NormalizeKey(dto.Name) ?? dto.Name.ToUpperInvariant();
            var role = new IdentityRole
            {
                Name = dto.Name,
                NormalizedName = normalized,
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to create role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role created",
                Data = MapRole(role)
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(string id, [FromBody] RoleUpdateDto dto)
        {
            LogCurrentUser(nameof(UpdateAsync));
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid role payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            var existingWithName = await _roleManager.FindByNameAsync(dto.Name);
            if (existingWithName != null && existingWithName.Id != role.Id)
            {
                return Conflict(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role name already exists",
                    ErrorCode = "ROLE_EXISTS"
                });
            }

            role.Name = dto.Name;
            role.NormalizedName = _roleManager.NormalizeKey(dto.Name) ?? dto.Name.ToUpperInvariant();
            role.ConcurrencyStamp = Guid.NewGuid().ToString();

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to update role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role updated",
                Data = MapRole(role)
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            LogCurrentUser(nameof(DeleteAsync));
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            var hasAssignments = await _context.UserRoles.AnyAsync(ur => ur.RoleId == id);
            if (hasAssignments)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cannot delete a role that is assigned to users.",
                    ErrorCode = "ROLE_IN_USE"
                });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<IEnumerable<string>>
                {
                    Success = false,
                    Message = "Failed to delete role",
                    ErrorCode = "IDENTITY_ERROR",
                    Data = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role deleted"
            });
        }

        private static RoleDto MapRole(IdentityRole role) => new RoleDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            NormalizedName = role.NormalizedName ?? string.Empty
        };

        private void LogCurrentUser(string action)
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userName = User?.Identity?.Name ?? "anonymous";
            var roles = User?.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToArray() ?? Array.Empty<string>();

            _logger.LogInformation("RolesController.{Action} invoked by {UserId}/{UserName} with roles [{Roles}]", action, userId, userName, string.Join(", ", roles));
        }
    }
}
