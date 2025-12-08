using library_management_system_backend.Data;
using library_management_system_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace library_management_system_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext appDbContext, IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDTO userDTO)
        {
            if (string.IsNullOrWhiteSpace(userDTO.name) || 
                string.IsNullOrWhiteSpace(userDTO.email) || 
                string.IsNullOrWhiteSpace(userDTO.password))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            var existingUser = await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.email == userDTO.email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "User with this email already exists" });
            }

            var newUser = new User
            {
                name = userDTO.name,
                email = userDTO.email,
                password = userDTO.password  // WARNING: In production, hash the password!
            };

            _appDbContext.Users.Add(newUser);
            await _appDbContext.SaveChangesAsync();

            return Ok(new 
            { 
                message = "User registered successfully", 
                userId = newUser.id,
                name = newUser.name,
                email = newUser.email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.email == loginDTO.email && x.password == loginDTO.password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            var jwtSubject = _configuration["Jwt:Subject"] ?? "DefaultSubject";
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured");
            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtSubject),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.id.ToString()),
                new Claim("Email", user.email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: signIn
            );

            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            
            return Ok(new 
            { 
                token = tokenValue, 
                data = new 
                { 
                    id = user.id, 
                    name = user.name, 
                    email = user.email 
                } 
            });
        }
    }
}
