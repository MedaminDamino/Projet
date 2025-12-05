using API.DTO;
using API.Models;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if(await userManager.FindByNameAsync(registerDTO.Username)!=null)
            {
                return BadRequest("username Existe !!");
            }
            else
            {
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = registerDTO.Username,
                    Email = registerDTO.Email
                };
               var result =await userManager.CreateAsync(applicationUser,
                    registerDTO.Password);
                if (result.Succeeded)
                {
                    return Created();
                }
                return BadRequest("Problème de création");
            }
           
        }
        [HttpPost("login")]
        public async Task<IActionResult>  login(LoginDTO loginDTO)
        {
            var user = await userManager.FindByNameAsync(loginDTO.Username);
            if(user==null)
            {
                return Unauthorized("username or password incorrect");
            }
            if(await userManager.CheckPasswordAsync(user,loginDTO.Password))
            {
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name,loginDTO.Username ));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()));

                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                    configuration["JWT:SecretKey"]));

                var sc=new SigningCredentials(key, SecurityAlgorithms.HmacSha256 );


                var token = new JwtSecurityToken(
                    audience: configuration["JWT:audience"],
                    issuer: configuration["JWT:issuer"],
                    claims: claims,
                    signingCredentials:sc,
                    expires: DateTime.Now.AddHours(1)
                    );

                var _token = new
                {

                token= new JwtSecurityTokenHandler().WriteToken(token),
                expiration= token.ValidTo,
                username= loginDTO.Username,
                };        

                return Ok( _token );    
            }
            return Unauthorized("invalid crendentials");
        }
    }
}
