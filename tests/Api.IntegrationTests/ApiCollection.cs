namespace Api.IntegrationTests;

/// <summary>
/// Todas las clases de prueba de la API comparten una sola instancia de la API
/// y un solo contenedor de PostgreSQL (se arranca una vez por ejecución).
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<PostgresApiFactory>
{
    public const string Name = "api";
}
