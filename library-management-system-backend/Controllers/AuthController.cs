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
				return BadRequest(new { message = "All fields are required" });

			var existingUser = await _appDbContext.Users
				.FirstOrDefaultAsync(u => u.email == userDTO.email);

			if (existingUser != null)
				return BadRequest(new { message = "User with this email already exists" });

			var newUser = new User
			{
				name = userDTO.name,
				email = userDTO.email,
				password = userDTO.password 
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
			if (string.IsNullOrWhiteSpace(loginDTO.email) ||
				string.IsNullOrWhiteSpace(loginDTO.password))
				return BadRequest(new { message = "Email and password are required" });

			var user = await _appDbContext.Users
				.FirstOrDefaultAsync(u => u.email == loginDTO.email);

			if (user == null || user.password != loginDTO.password)
				return Unauthorized(new { message = "Invalid email or password" });

			var token = GenerateJwtToken(user);

			return Ok(new
			{
				message = "Login successful",
				token,
				userId = user.id,
				name = user.name,
				email = user.email
			});
		}

		private string GenerateJwtToken(User user)
		{
			var jwtKey = _configuration["Jwt:Key"]
				?? throw new InvalidOperationException("JWT Key is not configured");

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new []
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
				new Claim(JwtRegisteredClaimNames.Email, user.email),
				new Claim(JwtRegisteredClaimNames.Name, user.name),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddHours(24),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
