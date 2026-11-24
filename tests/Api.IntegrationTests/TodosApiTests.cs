using System.Net;
using System.Net.Http.Json;

namespace Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class TodosApiTests(PostgresApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private sealed record TodoDto(int Id, string Title, bool Completed, DateTime CreatedAt);
    private sealed record ValidationErrors(Dictionary<string, string[]> Errors);

    private async Task<TodoDto> CreateTodoAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TodoDto>())!;
    }

    [Fact]
    public async Task Post_ConTituloValido_CreaLaTarea()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = "  Preparar la entrega  " });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>();
        Assert.NotNull(todo);
        Assert.Equal("Preparar la entrega", todo.Title);
        Assert.False(todo.Completed);
        Assert.Equal($"/api/todos/{todo.Id}", response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_SinTitulo_Devuelve400(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationErrors>();
        Assert.Contains("title", problem!.Errors.Keys);
    }

    [Fact]
    public async Task Post_TituloDemasiadoLargo_Devuelve400()
    {
        var response = await _client.PostAsJsonAsync("/api/todos", new { title = new string('x', 201) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPorId_TareaExistente_LaDevuelve()
    {
        var created = await CreateTodoAsync("Leer la guía del TFE");

        var todo = await _client.GetFromJsonAsync<TodoDto>($"/api/todos/{created.Id}");

        Assert.Equal(created.Id, todo!.Id);
        Assert.Equal("Leer la guía del TFE", todo.Title);
    }

    [Fact]
    public async Task GetPorId_TareaInexistente_Devuelve404()
    {
        var response = await _client.GetAsync($"/api/todos/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_MarcarComoCompletada_SeGuardaEnLaBaseDeDatos()
    {
        var created = await CreateTodoAsync("Configurar el pipeline");

        var response = await _client.PutAsJsonAsync($"/api/todos/{created.Id}", new { completed = true });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stored = await _client.GetFromJsonAsync<TodoDto>($"/api/todos/{created.Id}");
        Assert.True(stored!.Completed);
        Assert.Equal("Configurar el pipeline", stored.Title);
    }

    [Fact]
    public async Task Put_TareaInexistente_Devuelve404()
    {
        var response = await _client.PutAsJsonAsync($"/api/todos/{int.MaxValue}", new { completed = true });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_TareaExistente_LaElimina()
    {
        var created = await CreateTodoAsync("Tarea para borrar");

        var response = await _client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var after = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, after.StatusCode);
    }

    [Fact]
    public async Task GetLista_IncluyeLasTareasCreadas()
    {
        var a = await CreateTodoAsync("Lista A");
        var b = await CreateTodoAsync("Lista B");

        var todos = await _client.GetFromJsonAsync<List<TodoDto>>("/api/todos?limit=100");

        Assert.Contains(todos!, t => t.Id == a.Id);
        Assert.Contains(todos!, t => t.Id == b.Id);
    }

    [Theory]
    [InlineData("/api/todos?offset=-1")]
    [InlineData("/api/todos?limit=0")]
    [InlineData("/api/todos?limit=101")]
    public async Task GetLista_PaginacionInvalida_Devuelve400(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
