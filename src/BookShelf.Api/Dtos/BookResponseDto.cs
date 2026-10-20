using BookShelf.Api.Models;

namespace BookShelf.Api.Dtos;

/// <summary>
/// Datos que la API devuelve para un libro.
/// </summary>
// Contrato de salida: lo que el cliente ve, independiente de la entidad de EF.
public class BookResponseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public int PublicationYear { get; set; }

    public ReadingStatus ReadingStatus { get; set; }

    public int? Rating { get; set; }

    /// <summary>Fecha de creación del registro, en UTC.</summary>
    // Se serializa en ISO 8601 con Z porque el mapeo lo marca como UTC.
    public DateTime CreatedAt { get; set; }
}
