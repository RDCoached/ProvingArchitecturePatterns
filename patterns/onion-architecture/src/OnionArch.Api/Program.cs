using Microsoft.EntityFrameworkCore;
using OnionArch.Api.Endpoints;
using OnionArch.Api.Extensions;
using OnionArch.Application.Interfaces;
using OnionArch.Infrastructure.Persistence;
using OnionArch.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Database"),
        b => b.MigrationsAssembly("OnionArch.Infrastructure")));

// Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// Command/Query Handlers - registered automatically via extension method
builder.Services.AddApplicationHandlers();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Apply migrations automatically in development
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapOrderEndpoints();

app.Run();
