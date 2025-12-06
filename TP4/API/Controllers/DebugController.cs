using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        [HttpGet("me")]
        [AllowAnonymous]
        public IActionResult GetCurrentPrincipal()
        {
            var user = HttpContext.User;
            var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
            var name = user?.Identity?.Name ?? "anonymous";
            var claims = user?.Claims.Select(c => new { c.Type, c.Value }).ToList() ?? new List<object>();

            _logger.LogInformation("Debug/me requested. Authenticated={Authenticated}, Name={Name}, Claims={Claims}", isAuthenticated, name, string.Join(", ", claims.Select(c => $"{c}")));

            return Ok(new
            {
                isAuthenticated,
                name,
                claims
            });
        }
    }
}
