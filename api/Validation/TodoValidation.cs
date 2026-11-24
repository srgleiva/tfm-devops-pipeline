using Api.Models;

namespace Api.Validation;

/// <summary>
/// Reglas de validación de la API de tareas, separadas de los endpoints
/// para poder probarlas con pruebas unitarias.
/// </summary>
public static class TodoValidation
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 100;

    /// <summary>Devuelve el mensaje de error, o null si el título es válido.</summary>
    public static string? ValidateTitle(string? title, out string normalizedTitle)
    {
        normalizedTitle = title?.Trim() ?? string.Empty;

        if (normalizedTitle.Length == 0)
        {
            return "Title is required.";
        }

        if (normalizedTitle.Length > Todo.MaxTitleLength)
        {
            return $"Title must be {Todo.MaxTitleLength} characters or fewer.";
        }

        return null;
    }

    /// <summary>Devuelve el campo y el mensaje de error, o null si la paginación es válida.</summary>
    public static (string Field, string Message)? ValidatePagination(
        int? offset, int? limit, out int normalizedOffset, out int normalizedLimit)
    {
        normalizedOffset = offset ?? 0;
        normalizedLimit = limit ?? DefaultLimit;

        if (normalizedOffset < 0)
        {
            return ("offset", "Offset must be greater than or equal to 0.");
        }

        if (normalizedLimit is < 1 or > MaxLimit)
        {
            return ("limit", $"Limit must be between 1 and {MaxLimit}.");
        }

        return null;
    }
}
