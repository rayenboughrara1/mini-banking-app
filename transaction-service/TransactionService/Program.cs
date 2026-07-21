using Scalar.AspNetCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

var transactions = new List<Transaction>();
var nextId = 1;

app.MapGet("/transactions", () => transactions)
    .WithName("GetAllTransactions")
    .RequireAuthorization();

app.MapPost("/transactions", async (Transaction transaction) =>
{
    transaction.Id = nextId++;
    transaction.Timestamp = DateTime.UtcNow;
    transactions.Add(transaction);

    var factory = new ConnectionFactory { HostName = "rabbitmq-service" };
    using var connection = await factory.CreateConnectionAsync();
    using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
        queue: "transactions-queue",
        durable: true,
        exclusive: false,
        autoDelete: false);

    var messageBody = JsonSerializer.Serialize(transaction);
    var body = Encoding.UTF8.GetBytes(messageBody);

    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "transactions-queue",
        body: body);

    return Results.Created($"/transactions/{transaction.Id}", transaction);
})
    .WithName("CreateTransaction")
    .RequireAuthorization();

app.Run();