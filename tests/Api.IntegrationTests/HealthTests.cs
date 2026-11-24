using System.Net;

namespace Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class HealthTests(PostgresApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Health_ConBaseDeDatosDisponible_DevuelveHealthy(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
