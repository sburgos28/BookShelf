using System.Reflection;
using System.Text.Json.Serialization;
using BookShelf.Api.Data;
using BookShelf.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

// Punto de entrada: registra los servicios y configura el pipeline HTTP.
var builder = WebApplication.CreateBuilder(args);

// La cadena se lee de appsettings, user-secrets o variables de entorno; nunca se escribe aquí.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'DefaultConnection'. " +
        "Configúrala con user-secrets (ver README.md).");
}

// Registra los controllers y hace que los enums viajen como texto ("Read").
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa los enums como texto ("Read") y rechaza valores numéricos.
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    });

// ProblemDetails es el formato estándar (RFC 7807) para los errores de la API.
builder.Services.AddProblemDetails();

// Normaliza los errores de validación (incluidos los de deserialización JSON)
// para que todas las respuestas 400 tengan el mismo formato y mensajes claros.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var (key, entry) in context.ModelState)
        {
            if (key == "request")
            {
                continue;
            }

            var isJsonError = key.StartsWith("$", StringComparison.Ordinal);
            var field = isJsonError ? key.TrimStart('$').TrimStart('.') : key;
            var normalizedField = string.IsNullOrEmpty(field) ? "body" : ToPascalCase(field);

            var messages = entry.Errors
                .Select(error => !isJsonError
                    ? error.ErrorMessage
                    : string.IsNullOrEmpty(field)
                        ? "El cuerpo de la petición no es válido."
                        : $"El valor del campo '{field}' no es válido.")
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();

            if (messages.Length > 0)
            {
                errors[normalizedField] = messages;
            }
        }

        if (errors.Count == 0)
        {
            errors["body"] = new[] { "El cuerpo de la petición no es válido." };
        }

        return new BadRequestObjectResult(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos de entrada no válidos",
            Instance = context.HttpContext.Request.Path
        });
    };
});

// Un DbContext por petición: EF Core abre y cierra la conexión a MySQL por nosotros.
builder.Services.AddDbContext<BookShelfDbContext>(options =>
    options.UseMySQL(connectionString));

// Scoped: una instancia por petición, compartiendo el DbContext de esa petición.
builder.Services.AddScoped<BookService>();

// Swagger documenta los controllers a partir de sus atributos y comentarios XML.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BookShelf API",
        Version = "v1",
        Description = "API REST para gestionar una biblioteca personal de libros."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Construye la aplicación con todos los servicios registrados.
var app = builder.Build();

// Convierte excepciones no controladas en un 500 con formato ProblemDetails.
app.UseExceptionHandler();

// Sirve la interfaz web de wwwroot en http://localhost:5080/
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookShelf API v1");
        options.DocumentTitle = "BookShelf API";
    });
}

// Activa el enrutado por atributos: [Route], [HttpGet], [HttpPost], etc.
app.MapControllers();

app.Run();

// Convierte "readingStatus" en "ReadingStatus" para mostrar nombres de campo claros.
static string ToPascalCase(string value)
{
    return string.IsNullOrEmpty(value)
        ? value
        : char.ToUpperInvariant(value[0]) + value[1..];
}
