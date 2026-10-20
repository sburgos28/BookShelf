using BookShelf.Api.Dtos;
using BookShelf.Api.Models;

namespace BookShelf.Api.Mappings;

/// <summary>
/// Conversiones entre la entidad <see cref="Book"/> y sus DTOs.
/// </summary>
public static class BookMappings
{
    // La API devuelve DTOs, no entidades: así no expone detalles de la base de datos.
    public static BookResponseDto ToResponseDto(this Book book)
    {
        return new BookResponseDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            Genre = book.Genre,
            PublicationYear = book.PublicationYear,
            ReadingStatus = book.ReadingStatus,
            Rating = book.Rating,
            // MySQL no conserva DateTimeKind; lo marcamos como UTC al leer.
            CreatedAt = DateTime.SpecifyKind(book.CreatedAt, DateTimeKind.Utc)
        };
    }

    public static Book ToEntity(this BookRequestDto dto)
    {
        var book = new Book();
        dto.ApplyTo(book);
        return book;
    }

    public static void ApplyTo(this BookRequestDto dto, Book book)
    {
        // Trim evita espacios accidentales; un género vacío se guarda como NULL.
        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.Genre = string.IsNullOrWhiteSpace(dto.Genre) ? null : dto.Genre.Trim();
        book.PublicationYear = dto.PublicationYear;

        // El DTO ya pasó la validación, por eso podemos usar .Value con seguridad.
        book.ReadingStatus = dto.ReadingStatus!.Value;
        book.Rating = dto.Rating;
    }
}
