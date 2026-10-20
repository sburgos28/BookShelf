using BookShelf.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookShelf.Api.Data.Configurations;

/// <summary>
/// Configuración de la entidad <see cref="Book"/> en la base de datos.
/// </summary>
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // Fluent API define el esquema; las migraciones se generan a partir de esto.
        builder.ToTable("books");

        // Clave primaria autoincremental generada por MySQL.
        builder.HasKey(book => book.Id);

        builder.Property(book => book.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(book => book.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(book => book.Author)
            .HasColumnName("author")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(book => book.Genre)
            .HasColumnName("genre")
            .HasMaxLength(100);

        builder.Property(book => book.PublicationYear)
            .HasColumnName("publication_year")
            .IsRequired();

        // HasConversion<string>() guarda "Pending"/"Reading"/"Read" en vez de 1/2/3.
        builder.Property(book => book.ReadingStatus)
            .HasColumnName("reading_status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(book => book.Rating)
            .HasColumnName("rating");

        // datetime(6) conserva microsegundos; la aplicación trabaja siempre en UTC.
        builder.Property(book => book.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime(6)")
            .IsRequired();
    }
}
