using API.DTO;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public AccountController
            (UserManager<ApplicationUser> userManager ,IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid data." : e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Validation failed",
                    ErrorCode = "validation_error",
                    Data = errors
                });
            }

            if (await userManager.FindByNameAsync(registerDTO.Username) != null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Username already exists.",
                    ErrorCode = "username_exists"
                });
            }

            if (!string.IsNullOrWhiteSpace(registerDTO.Email) &&
                await userManager.FindByEmailAsync(registerDTO.Email) != null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Email already in use.",
                    ErrorCode = "email_exists"
                });
            }

            var applicationUser = new ApplicationUser
            {
                UserName = registerDTO.Username,
                Email = registerDTO.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(applicationUser, registerDTO.Password);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Registration successful."
                });
            }

            var identityErrors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(new ApiResponse<List<string>>
            {
                Success = false,
                Message = "Registration failed.",
                ErrorCode = "identity_error",
                Data = identityErrors
            });
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Validation failed",
                    ErrorCode = "validation_error",
                    Data = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            // Allow login with username or email
            var user = await userManager.FindByNameAsync(loginDTO.Username)
                       ?? await userManager.FindByEmailAsync(loginDTO.Username);

            if (user == null || !await userManager.CheckPasswordAsync(user, loginDTO.Password))
            {
                return Unauthorized(new ApiResponse
                {
                    Success = false,
                    Message = "Username or password incorrect",
                    ErrorCode = "invalid_credentials"
                });
            }

            // Debug: Log user info
            Console.WriteLine($"=== [AccountController] Login for user: {user.UserName} (ID: {user.Id}) ===");

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? loginDTO.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Get roles and add to claims
            var roles = await userManager.GetRolesAsync(user);
            Console.WriteLine($"[AccountController] GetRolesAsync returned {roles.Count} roles:");
            foreach (var role in roles)
            {
                Console.WriteLine($"  - Role: '{role}'");
                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }
            
            if (roles.Count == 0)
            {
                Console.WriteLine("[AccountController] WARNING: No roles found for this user!");
            }

            Console.WriteLine($"[AccountController] Total claims being added to token: {claims.Count}");
            foreach (var claim in claims)
            {
                Console.WriteLine($"  - Claim: Type='{claim.Type}', Value='{claim.Value}'");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                configuration["JWT:SecretKey"]));

            var sc = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                audience: configuration["JWT:audience"],
                issuer: configuration["JWT:issuer"],
                claims: claims,
                signingCredentials: sc,
                expires: DateTime.Now.AddHours(1)
                );

            var _token = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                username = user.UserName ?? loginDTO.Username,
                roles = roles.ToList() // Include roles in response for debugging
            };

            Console.WriteLine($"[AccountController] Token generated. Returning with {roles.Count} roles.");

            return Ok(_token);
        }
    }
}
