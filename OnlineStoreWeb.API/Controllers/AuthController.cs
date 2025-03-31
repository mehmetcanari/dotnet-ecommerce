using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineStoreWeb.API.DTO.Request.Account;

namespace OnlineStoreWeb.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IConfiguration configuration, 
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CreateUser([FromBody] AccountRegisterDto accountRegisterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(accountRegisterDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("Email is already in use.");
                }

                var user = new IdentityUser
                {
                    UserName = accountRegisterDto.Email,
                    Email = accountRegisterDto.Email
                };

                var result = await _userManager.CreateAsync(user, accountRegisterDto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(result.Errors);
                }

                const string defaultRole = "User";

                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to create role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        return BadRequest("Error creating role.");
                    }
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to add role to user: {Errors}", string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    return BadRequest(addRoleResult.Errors);
                }

                _logger.LogInformation("User {Email} registered successfully", accountRegisterDto.Email);
                return Ok(new { Message = "User created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AccountLoginDto accountLoginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for user: {Email}", accountLoginDto.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(accountLoginDto.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed - User not found: {Email}", accountLoginDto.Email);
                    return BadRequest("Invalid email or password.");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, accountLoginDto.Password);
                if (!isPasswordValid)
                {
                    _logger.LogWarning("Login failed - Invalid password for user: {Email}", accountLoginDto.Email);
                    return BadRequest("Invalid email or password.");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user.Email, roles);

                _logger.LogInformation("User {Email} logged in successfully", accountLoginDto.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", accountLoginDto.Email);
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] AccountRegisterDto accountRegisterDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingUser = await _userManager.FindByEmailAsync(accountRegisterDto.Email);
                if (existingUser != null)
                {
                    return BadRequest("Email is already in use.");
                }

                var user = new IdentityUser
                {
                    UserName = accountRegisterDto.Email,
                    Email = accountRegisterDto.Email
                };

                var result = await _userManager.CreateAsync(user, accountRegisterDto.Password);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create admin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(result.Errors);
                }

                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    if (!roleResult.Succeeded)
                    {
                        _logger.LogError("Failed to create admin role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        return BadRequest("Error creating admin role.");
                    }
                }

                var addRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!addRoleResult.Succeeded)
                {
                    _logger.LogError("Failed to add admin role to user: {Errors}", string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    return BadRequest(addRoleResult.Errors);
                }

                _logger.LogInformation("Admin user {Email} created successfully", accountRegisterDto.Email);
                return Ok(new { Message = "Admin user created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin creation");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        private string GenerateJwtToken(string email, IList<string> roles)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}
