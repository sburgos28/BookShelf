namespace BookShelf.Api.Models;

/// <summary>
/// Estado de lectura de un libro.
/// </summary>
// Valores explícitos por claridad; en MySQL se guarda el nombre, no el número.
public enum ReadingStatus
{
    /// <summary>El libro está pendiente de lectura.</summary>
    Pending = 1,

    /// <summary>El libro se está leyendo actualmente.</summary>
    Reading = 2,

    /// <summary>El libro ya se ha leído.</summary>
    Read = 3
}
