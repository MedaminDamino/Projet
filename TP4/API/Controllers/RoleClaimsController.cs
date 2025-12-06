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
    public class RoleClaimsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RoleClaimsController> _logger;

        public RoleClaimsController(ApplicationContext context, RoleManager<IdentityRole> roleManager, ILogger<RoleClaimsController> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetByRoleAsync(string roleId)
        {
            LogCurrentUser(nameof(GetByRoleAsync));
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            var claims = await _context.RoleClaims
                .Where(c => c.RoleId == roleId)
                .OrderBy(c => c.ClaimType)
                .ThenBy(c => c.ClaimValue)
                .Select(c => new RoleClaimDto
                {
                    Id = c.Id,
                    RoleId = c.RoleId,
                    ClaimType = c.ClaimType ?? string.Empty,
                    ClaimValue = c.ClaimValue ?? string.Empty
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<RoleClaimDto>>
            {
                Success = true,
                Message = "Claims loaded",
                Data = claims
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] RoleClaimCreateDto dto)
        {
            LogCurrentUser(nameof(CreateAsync));
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid claim payload",
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var role = await _roleManager.FindByIdAsync(dto.RoleId);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found",
                    ErrorCode = "ROLE_NOT_FOUND"
                });
            }

            var claim = new IdentityRoleClaim<string>
            {
                RoleId = dto.RoleId,
                ClaimType = dto.ClaimType,
                ClaimValue = dto.ClaimValue
            };

            await _context.RoleClaims.AddAsync(claim);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<RoleClaimDto>
            {
                Success = true,
                Message = "Claim created",
                Data = new RoleClaimDto
                {
                    Id = claim.Id,
                    RoleId = claim.RoleId,
                    ClaimType = claim.ClaimType ?? string.Empty,
                    ClaimValue = claim.ClaimValue ?? string.Empty
                }
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            LogCurrentUser(nameof(DeleteAsync));
            var claim = await _context.RoleClaims.FirstOrDefaultAsync(c => c.Id == id);
            if (claim == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role claim not found",
                    ErrorCode = "ROLE_CLAIM_NOT_FOUND"
                });
            }

            _context.RoleClaims.Remove(claim);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Claim deleted"
            });
        }

        private void LogCurrentUser(string action)
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            var userName = User?.Identity?.Name ?? "anonymous";
            var roles = User?.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .ToArray() ?? Array.Empty<string>();

            _logger.LogInformation("RoleClaimsController.{Action} invoked by {UserId}/{UserName} with roles [{Roles}]", action, userId, userName, string.Join(", ", roles));
        }
    }
}
