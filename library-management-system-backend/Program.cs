using library_management_system_backend.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Create Web Application Builder
var builder = WebApplication.CreateBuilder(args);

// Configure URLs
builder.WebHost.UseUrls("http://localhost:5119", "https://localhost:7163");

// ================================
// CORS Configuration (FIXED)
// ================================
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontendApp", policy =>
	{
		policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	});
});

// Add services to container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ================================
// JWT Authentication
// ================================
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	var jwtKey = builder.Configuration["Jwt:Key"]
		?? throw new InvalidOperationException("JWT Key is not configured");

	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
	};
});

// ================================
// Database Context (SQLite)
// ================================
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ================================
// Build App
// ================================
var app = builder.Build();

// ================================
// Middleware Order (FIXED)
// ================================

// Enable Developer Exception Page in Development to see detailed errors
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.MapOpenApi();
}

// CORS must come BEFORE Authentication/Authorization
app.UseCors("AllowFrontendApp");

// Only use HTTPS redirection in Production
if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
