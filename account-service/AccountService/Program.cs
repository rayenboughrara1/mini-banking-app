cd C:\Users\bogho\OneDrive\Desktop\mini-banking-app\transaction-service\TransactionService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var accounts = new List<Account>
{
    new Account { Id = 1, OwnerName = "Ahmed Ben Salah", Balance = 1500.50m },
    new Account { Id = 2, OwnerName = "Fatma Trabelsi", Balance = 3200.00m },
    new Account { Id = 3, OwnerName = "Karim Jaziri", Balance = 750.25m }
};

app.MapGet("/accounts", () => accounts)
    .WithName("GetAllAccounts")
    .RequireAuthorization();

app.MapGet("/accounts/{id}", (int id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    return account is not null ? Results.Ok(account) : Results.NotFound();
})
    .WithName("GetAccountById")
    .RequireAuthorization();

app.Run();