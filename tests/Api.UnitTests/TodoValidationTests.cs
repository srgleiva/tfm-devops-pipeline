using Api.Models;
using Api.Validation;

namespace Api.UnitTests;

public class TodoValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateTitle_SinTexto_DevuelveError(string? title)
    {
        var error = TodoValidation.ValidateTitle(title, out var normalized);

        Assert.Equal("Title is required.", error);
        Assert.Equal(string.Empty, normalized);
    }

    [Fact]
    public void ValidateTitle_ConEspacios_LosElimina()
    {
        var error = TodoValidation.ValidateTitle("  Comprar pan  ", out var normalized);

        Assert.Null(error);
        Assert.Equal("Comprar pan", normalized);
    }

    [Fact]
    public void ValidateTitle_EnElLimite_EsValido()
    {
        var title = new string('a', Todo.MaxTitleLength);

        var error = TodoValidation.ValidateTitle(title, out var normalized);

        Assert.Null(error);
        Assert.Equal(Todo.MaxTitleLength, normalized.Length);
    }

    [Fact]
    public void ValidateTitle_PorEncimaDelLimite_DevuelveError()
    {
        var title = new string('a', Todo.MaxTitleLength + 1);

        var error = TodoValidation.ValidateTitle(title, out _);

        Assert.Equal($"Title must be {Todo.MaxTitleLength} characters or fewer.", error);
    }

    [Fact]
    public void ValidateTitle_LosEspaciosNoCuentanParaElLimite()
    {
        var title = "  " + new string('a', Todo.MaxTitleLength) + "  ";

        var error = TodoValidation.ValidateTitle(title, out _);

        Assert.Null(error);
    }

    [Fact]
    public void ValidatePagination_SinParametros_UsaValoresPorDefecto()
    {
        var error = TodoValidation.ValidatePagination(null, null, out var offset, out var limit);

        Assert.Null(error);
        Assert.Equal(0, offset);
        Assert.Equal(TodoValidation.DefaultLimit, limit);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(10, 50)]
    [InlineData(0, TodoValidation.MaxLimit)]
    public void ValidatePagination_ValoresValidos_NoDevuelveError(int offset, int limit)
    {
        var error = TodoValidation.ValidatePagination(offset, limit, out var o, out var l);

        Assert.Null(error);
        Assert.Equal(offset, o);
        Assert.Equal(limit, l);
    }

    [Fact]
    public void ValidatePagination_OffsetNegativo_DevuelveError()
    {
        var error = TodoValidation.ValidatePagination(-1, 10, out _, out _);

        Assert.NotNull(error);
        Assert.Equal("offset", error.Value.Field);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(TodoValidation.MaxLimit + 1)]
    public void ValidatePagination_LimitFueraDeRango_DevuelveError(int limit)
    {
        var error = TodoValidation.ValidatePagination(0, limit, out _, out _);

        Assert.NotNull(error);
        Assert.Equal("limit", error.Value.Field);
    }
}
