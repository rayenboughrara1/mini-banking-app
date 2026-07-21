using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Hardcoded admin for now (in a real system, this would come from a database)
var admins = new List<Admin>
{
    new Admin
    {
        Id = 1,
        Username = "admin",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
    }
};

app.MapPost("/login", (LoginRequest request) =>
{
    var admin = admins.FirstOrDefault(a => a.Username == request.Username);

    if (admin is null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var jwtKey = builder.Configuration["Jwt:Key"]!;
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
    var jwtAudience = builder.Configuration["Jwt:Audience"]!;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, admin.Username),
        new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString())
    };

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: credentials
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new LoginResponse { Token = tokenString });
})
    .WithName("Login");

app.Run();