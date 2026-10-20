using BookShelf.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookShelf.Api.Data;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos de BookShelf.
/// </summary>
public class BookShelfDbContext : DbContext
{
    public BookShelfDbContext(DbContextOptions<BookShelfDbContext> options)
        : base(options)
    {
    }

    // Cada DbSet representa una tabla; las consultas LINQ se traducen a SQL.
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carga todas las configuraciones IEntityTypeConfiguration del ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookShelfDbContext).Assembly);
    }
}
