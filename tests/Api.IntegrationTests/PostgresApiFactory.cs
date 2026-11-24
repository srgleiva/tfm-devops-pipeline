using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Api.IntegrationTests;

/// <summary>
/// Arranca la API real en memoria (WebApplicationFactory) contra un PostgreSQL
/// de verdad que Testcontainers levanta en Docker solo para las pruebas.
/// </summary>
public sealed class PostgresApiFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    public PostgresApiFactory()
    {
        // Se arranca de forma síncrona para no depender de la versión de IAsyncLifetime de xUnit.
        _postgres.StartAsync().GetAwaiter().GetResult();

        // La API lee la cadena de conexión al construirse, así que se pasa también como variable de entorno.
        Environment.SetEnvironmentVariable("ConnectionStrings__db", _postgres.GetConnectionString());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:db", _postgres.GetConnectionString());
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
