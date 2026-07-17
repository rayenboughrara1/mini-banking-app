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

var accounts = new List<Account>
{
    new Account { Id = 1, OwnerName = "Ahmed Ben Salah", Balance = 1500.50m },
    new Account { Id = 2, OwnerName = "Fatma Trabelsi", Balance = 3200.00m },
    new Account { Id = 3, OwnerName = "Karim Jaziri", Balance = 750.25m }
};

app.MapGet("/accounts", () => accounts)
    .WithName("GetAllAccounts");

app.MapGet("/accounts/{id}", (int id) =>
{
    var account = accounts.FirstOrDefault(a => a.Id == id);
    return account is not null ? Results.Ok(account) : Results.NotFound();
})
    .WithName("GetAccountById");

app.Run();