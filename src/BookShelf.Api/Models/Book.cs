namespace BookShelf.Api.Models;

/// <summary>
/// Entidad que representa un libro de la biblioteca personal.
/// </summary>
// Entidad de persistencia: representa una fila de la tabla books.
public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public int PublicationYear { get; set; }

    // Enum definido en ReadingStatus.cs; EF lo guarda como texto.
    public ReadingStatus ReadingStatus { get; set; }

    public int? Rating { get; set; }

    public DateTime CreatedAt { get; set; }
}
