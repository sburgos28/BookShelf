using System.ComponentModel.DataAnnotations;
using BookShelf.Api.Models;

namespace BookShelf.Api.Dtos;

/// <summary>
/// Datos de entrada para crear o actualizar un libro.
/// </summary>
public class BookRequestDto : IValidatableObject
{
    // Este DTO solo contiene los campos que el cliente puede enviar: no Id ni CreatedAt.
    /// <summary>Título del libro.</summary>
    /// <example>Dune</example>
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Autor del libro.</summary>
    /// <example>Frank Herbert</example>
    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(150, ErrorMessage = "El autor no puede superar los 150 caracteres.")]
    public string Author { get; set; } = string.Empty;

    /// <summary>Género literario del libro (opcional).</summary>
    /// <example>Ciencia ficción</example>
    [StringLength(100, ErrorMessage = "El género no puede superar los 100 caracteres.")]
    public string? Genre { get; set; }

    // El máximo depende del año actual; se valida en Validate() y no con [Range].
    /// <summary>Año de publicación. Debe ser un año real, no posterior al actual.</summary>
    /// <example>1965</example>
    public int PublicationYear { get; set; }

    // Nullable para distinguir "no enviado" de un valor válido; [Required] lo exige.
    /// <summary>Estado de lectura: Pending, Reading o Read.</summary>
    /// <example>Read</example>
    [Required(ErrorMessage = "El estado de lectura es obligatorio.")]
    [EnumDataType(typeof(ReadingStatus), ErrorMessage = "El estado de lectura no es válido. Use Pending, Reading o Read.")]
    public ReadingStatus? ReadingStatus { get; set; }

    // Al ser nullable, la valoración es opcional; [Range] valida 1..5 si existe.
    /// <summary>Valoración personal entre 1 y 5 (opcional).</summary>
    /// <example>5</example>
    [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5.")]
    public int? Rating { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // IValidatableObject se ejecuta después de las DataAnnotations.
        var currentYear = DateTime.UtcNow.Year;

        if (PublicationYear < 1 || PublicationYear > currentYear)
        {
            yield return new ValidationResult(
                $"El año de publicación debe estar entre 1 y {currentYear}.",
                new[] { nameof(PublicationYear) });
        }
    }
}
