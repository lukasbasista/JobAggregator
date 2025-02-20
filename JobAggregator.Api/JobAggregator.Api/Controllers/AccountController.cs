using JobAggregator.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobAggregator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Registers new user with provided registration data.
        /// </summary>
        /// <param name="model">Registration data (username, email, first name, last name, location, and password).</param>
        /// <returns>IActionResult indicating success or failure.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Location = model.Location,
                AccountCreated = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Authenticates a user with the provided credentials and returns a JWT token upon successful login.
        /// </summary>
        /// <param name="model">Login credentials (username and password).</param>
        /// <returns>IActionResult containing the JWT token if successful; otherwise Unauthorized.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Retrieves profile of currently authenticated user.
        /// </summary>
        /// <returns>IActionResult containing user profile data if found; otherwise NotFound or Unauthorized.</returns>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var profile = new UserProfileDTO
            {
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Location = user.Location,
                PhoneNumber = user.PhoneNumber,
                AccountCreated = user.AccountCreated,
                LastLogin = user.LastLogin
            };

            return Ok(profile);
        }

        /// <summary>
        /// Updates profile of the currently authenticated user with the provided profile data.
        /// </summary>
        /// <param name="model">new profile data (first name, last name, location, phone number).</param>
        /// <returns>IActionResult indicating result of the update operation.</returns>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UserProfileDTO model)
        {
            var userId = User.FindFirst("id")?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Location = model.Location;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">ApplicationUser for whom to generate the token.</param>
        /// <returns>JWT token as a string.</returns>
        private string GenerateJwtToken(ApplicationUser user)
        {
            var userClaims = new List<Claim>
            {
                new Claim("id", user.Id!),
                new Claim("username", user.UserName!)
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            var roleClaims = roles.Select(role => new Claim("role", role));

            var claims = userClaims.Concat(roleClaims);

            var identity = new ClaimsIdentity(claims);

            var secretKey = _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

}
