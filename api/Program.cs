using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Api.Data;
using Api.Extensions;
using Api.Services;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL con EF Core. La cadena de conexión se lee de ConnectionStrings:db
// (appsettings o variable de entorno ConnectionStrings__db).
var connectionString = builder.Configuration.GetConnectionString("db")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:db'.");

builder.Services.AddDbContext<TodoDbContext>(options => options.UseNpgsql(connectionString));

// Comprobaciones de salud: /health/live (el proceso responde) y /health/ready (además, la base de datos está disponible).
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TodoDbContext>("database", tags: ["ready"]);

builder.Services.AddOpenApi();

// Crea o migra la base de datos al arrancar.
builder.Services.AddHostedService<DatabaseInitializer>();

var app = builder.Build();

app.UseFileServer();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });

app.MapTodos();

app.Run();

// Permite usar WebApplicationFactory<Program> en las pruebas de integración.
public partial class Program;
