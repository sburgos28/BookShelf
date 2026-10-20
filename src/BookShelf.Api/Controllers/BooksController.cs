using BookShelf.Api.Dtos;
using BookShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookShelf.Api.Controllers;

/// <summary>
/// Operaciones CRUD sobre los libros de la biblioteca personal.
/// </summary>
[ApiController]
[Route("api/books")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    // El controller solo traduce HTTP <-> servicio; no contiene SQL ni reglas de persistencia.
    private readonly BookService _bookService;

    // Inyección de dependencias: ASP.NET Core crea y pasa BookService automáticamente.
    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>Obtiene todos los libros ordenados por título.</summary>
    /// <response code="200">Lista de libros.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BookResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BookResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookService.GetAllAsync(cancellationToken);
        return Ok(books);
    }

    /// <summary>Obtiene un libro por su identificador.</summary>
    /// <param name="id">Identificador del libro.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Libro encontrado.</response>
    /// <response code="404">No existe un libro con ese identificador.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIdAsync(id, cancellationToken);

        // El servicio devuelve null cuando no existe; aquí se traduce a 404.
        return book is null ? BookNotFound(id) : Ok(book);
    }

    /// <summary>Crea un libro.</summary>
    /// <response code="201">Libro creado. La cabecera Location apunta al recurso.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookResponseDto>> Create(
        BookRequestDto request,
        CancellationToken cancellationToken)
    {
        // [ApiController] ya validó el DTO; si hubiera errores, no se llegaría a esta línea.
        var createdBook = await _bookService.CreateAsync(request, cancellationToken);

        // 201 Created + cabecera Location: /api/books/{id}.
        return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
    }

    /// <summary>Actualiza todos los datos de un libro existente.</summary>
    /// <param name="id">Identificador del libro.</param>
    /// <param name="request">Nuevos datos del libro.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="200">Libro actualizado.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe un libro con ese identificador.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(BookResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookResponseDto>> Update(
        int id,
        BookRequestDto request,
        CancellationToken cancellationToken)
    {
        // PUT reemplaza todos los campos editables del libro.
        var updatedBook = await _bookService.UpdateAsync(id, request, cancellationToken);

        return updatedBook is null ? BookNotFound(id) : Ok(updatedBook);
    }

    /// <summary>Elimina un libro.</summary>
    /// <param name="id">Identificador del libro.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <response code="204">Libro eliminado.</response>
    /// <response code="404">No existe un libro con ese identificador.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        // 204 No Content es la respuesta habitual cuando se borra y no hay cuerpo.
        var deleted = await _bookService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : BookNotFound(id);
    }

    // Centraliza el formato del 404 para no repetirlo en cada endpoint.
    private NotFoundObjectResult BookNotFound(int id)
    {
        return NotFound(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Libro no encontrado",
            Detail = $"No existe ningún libro con el ID {id}.",
            Instance = HttpContext.Request.Path.Value
        });
    }
}
