using BookShelf.Api.Data;
using BookShelf.Api.Dtos;
using BookShelf.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Api.Services;

/// <summary>
/// Contiene la lógica de negocio y el acceso a datos de los libros.
/// </summary>
public class BookService
{
    // El service concentra la lógica de negocio y el acceso a datos.
    private readonly BookShelfDbContext _db;

    public BookService(BookShelfDbContext db)
    {
        _db = db;
    }

    // AsNoTracking acelera la lectura porque EF no necesita vigilar cambios.
    public async Task<IReadOnlyList<BookResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var books = await _db.Books
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);

        // Proyectamos a DTO en memoria para no devolver la entidad de persistencia.
        return books.Select(book => book.ToResponseDto()).ToList();
    }

    // FirstOrDefaultAsync devuelve null si no existe; el controller lo convierte en 404.
    public async Task<BookResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

        return book?.ToResponseDto();
    }

    public async Task<BookResponseDto> CreateAsync(BookRequestDto request, CancellationToken cancellationToken = default)
    {
        // Convertimos el DTO de entrada en la entidad que EF Core mapea a la tabla.
        var book = request.ToEntity();

        // La fecha la genera el servidor, no el cliente.
        book.CreatedAt = DateTime.UtcNow;

        // Add solo marca la entidad como nueva; el INSERT ocurre en SaveChangesAsync.
        _db.Books.Add(book);

        // SaveChangesAsync genera el INSERT y MySQL asigna el Id.
        await _db.SaveChangesAsync(cancellationToken);

        return book.ToResponseDto();
    }

    // En una actualización sí usamos tracking: EF debe detectar los cambios.
    public async Task<BookResponseDto?> UpdateAsync(
        int id,
        BookRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var book = await _db.Books
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

        if (book is null)
        {
            return null;
        }

        // ApplyTo copia solo los campos editables del DTO.
        request.ApplyTo(book);
        await _db.SaveChangesAsync(cancellationToken);

        return book.ToResponseDto();
    }

    // DeleteAsync usa tracking porque va a modificar el estado de la entidad.
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var book = await _db.Books
            .FirstOrDefaultAsync(book => book.Id == id, cancellationToken);

        if (book is null)
        {
            return false;
        }

        // Remove marca la fila para borrar; SaveChangesAsync genera el DELETE.
        _db.Books.Remove(book);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
