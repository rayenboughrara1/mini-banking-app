using Scalar.AspNetCore;

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

app.MapPost("/transactions", (Transaction transaction) =>
{
    transaction.Id = nextId++;
    transaction.Timestamp = DateTime.UtcNow;
    transactions.Add(transaction);
    return Results.Created($"/transactions/{transaction.Id}", transaction);
})
    .WithName("CreateTransaction");

app.Run();