using Scalar.AspNetCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var transactions = new List<Transaction>();
var nextId = 1;

app.MapGet("/transactions", () => transactions)
    .WithName("GetAllTransactions");

app.MapPost("/transactions", async (Transaction transaction) =>
{
    transaction.Id = nextId++;
    transaction.Timestamp = DateTime.UtcNow;
    transactions.Add(transaction);

    // Publish a message to RabbitMQ
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
    .WithName("CreateTransaction");

app.Run();