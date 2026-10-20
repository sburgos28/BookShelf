# BookShelf API

![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-512BD4)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4)
![MySQL](https://img.shields.io/badge/MySQL-8.4-4479A1)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D)

API REST para gestionar una biblioteca personal de libros. Permite crear, consultar, actualizar y eliminar libros desde una interfaz web o directamente contra la API, con validaciones y respuestas de error coherentes.

![Interfaz web de BookShelf](docs/images/bookshelf-ui.png)

**Estado:** v1. CRUD completo e interfaz web mínima. La búsqueda, los filtros y las estadísticas están planificados para la siguiente versión.

## Características

- CRUD completo de libros.
- Interfaz web para listar, crear, editar y eliminar libros.
- Validación de título, autor, año de publicación, estado de lectura y valoración.
- Listado ordenado por título y consulta por ID.
- Estado de lectura modelado como enum: `Pending`, `Reading` y `Read`.
- Respuestas de error homogéneas con `ProblemDetails` (RFC 7807).
- Documentación interactiva con Swagger UI.
- Acceso a datos asíncrono con Entity Framework Core.
- Migraciones para versionar el esquema de la base de datos.
- Credenciales de base de datos fuera del repositorio mediante user-secrets.

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 10.0 (LTS) | Plataforma de ejecución |
| ASP.NET Core Web API | 10.0 | Endpoints HTTP |
| Entity Framework Core | 10.0.9 | Acceso a datos y migraciones |
| MySql.EntityFrameworkCore | 10.0.9 | Proveedor oficial de MySQL para EF Core |
| MySQL | 8.4 LTS (probado con 8.4.10) | Base de datos relacional |
| Swashbuckle.AspNetCore | 10.3.1 | Swagger UI y documento OpenAPI |
| HTML, CSS y JavaScript | nativo | Interfaz web sin framework ni compilación |

### Versiones

- **.NET 10 es LTS** y tiene soporte hasta noviembre de 2028.
- **EF Core 10.0.9** se usa con el proveedor oficial `MySql.EntityFrameworkCore` 10.0.9. Pomelo todavía no publica una versión estable para EF Core 10.
- **Swashbuckle 10.3.1** genera el documento OpenAPI y sirve Swagger UI desde la misma aplicación.

## Estructura

```text
BookShelf/
├── BookShelf.sln
├── dotnet-tools.json          # Versión local de dotnet-ef
├── README.md
├── docs/images/               # Capturas
└── src/
    └── BookShelf.Api/
        ├── Controllers/       # Endpoints HTTP
        ├── Data/
        │   ├── BookShelfDbContext.cs
        │   ├── Configurations/  # Configuración Fluent API
        │   └── Migrations/      # Migraciones de EF Core
        ├── Dtos/              # Contratos de entrada y salida
        ├── Mappings/          # Conversión entre entidad y DTOs
        ├── Models/            # Entidad Book y enum ReadingStatus
        ├── Services/          # Lógica de negocio y consultas
        ├── wwwroot/           # Interfaz web (HTML, CSS y JS)
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        └── BookShelf.http     # Peticiones de ejemplo
```

Flujo de una petición:

```text
Navegador (HTML/CSS/JS)
   │ fetch('/api/books')
   ▼
BooksController → BookService → BookShelfDbContext → MySQL
```

El controller gestiona HTTP, el service contiene las consultas y la lógica de negocio, y los DTOs separan los contratos de la API de la entidad de persistencia.

## Requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (probado con 10.0.402).
- MySQL 8.0 o superior (probado con MySQL 8.4.10 LTS).
- `dotnet-ef` 10.0.9. El repositorio incluye un manifiesto local, así que basta con `dotnet tool restore`.
- La interfaz web no requiere Node ni un servidor aparte: son archivos estáticos servidos por la API.

## Instalación

### 1. Clonar el repositorio

```bash
git clone https://github.com/sburgos28/BookShelf.git
cd BookShelf
dotnet restore
dotnet tool restore
```

### 2. Crear la base de datos y el usuario

Conéctate a MySQL como administrador y ejecuta:

```sql
CREATE DATABASE IF NOT EXISTS bookshelf
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

CREATE USER IF NOT EXISTS 'bookshelf'@'localhost'
  IDENTIFIED BY 'TU_PASSWORD';

GRANT ALL PRIVILEGES ON bookshelf.* TO 'bookshelf'@'localhost';
FLUSH PRIVILEGES;
```

> `utf8mb4` permite tildes, eñes y emojis. La colación `utf8mb4_0900_ai_ci` es la predeterminada de MySQL 8.

### 3. Configurar la cadena de conexión

La cadena de conexión no se guarda en el repositorio. Se configura con user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Port=3306;Database=bookshelf;User ID=bookshelf;Password=TU_PASSWORD;SslMode=Preferred;" \
  --project src/BookShelf.Api
```

El proyecto ya incluye un `UserSecretsId` en el `.csproj`; no hace falta ejecutar `user-secrets init`.

> Si tu servidor MySQL escucha en otro puerto, cambia `Port=3306` por el que corresponda.

También puedes usar una variable de entorno:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=3306;Database=bookshelf;User ID=bookshelf;Password=TU_PASSWORD;SslMode=Preferred;"
```

> Para un entorno real, usa un usuario con privilegios mínimos, una contraseña robusta y TLS (`SslMode=Required` o superior). `SslMode=Preferred` facilita el desarrollo local.

### 4. Aplicar las migraciones

```bash
dotnet ef database update \
  --project src/BookShelf.Api \
  --startup-project src/BookShelf.Api
```

Esto crea la tabla `books` y la tabla de historial de migraciones de EF Core.

### 5. Ejecutar la aplicación

```bash
dotnet run --project src/BookShelf.Api
```

La aplicación escucha en `http://localhost:5080`.

- Interfaz web: <http://localhost:5080/>
- Swagger UI (en Development): <http://localhost:5080/swagger>
- Documento OpenAPI: <http://localhost:5080/swagger/v1/swagger.json>

## Endpoints

| Método | Ruta | Descripción | Respuesta correcta |
|---|---|---|---|
| `GET` | `/api/books` | Lista todos los libros ordenados por título | `200 OK` |
| `GET` | `/api/books/{id}` | Obtiene un libro por ID | `200 OK` |
| `POST` | `/api/books` | Crea un libro | `201 Created` + `Location` |
| `PUT` | `/api/books/{id}` | Actualiza todos los datos editables | `200 OK` |
| `DELETE` | `/api/books/{id}` | Elimina un libro | `204 No Content` |

Errores:

- `400 Bad Request`: datos inválidos. Devuelve un `ValidationProblemDetails` con los campos afectados.
- `404 Not Found`: el ID no existe. Devuelve un `ProblemDetails` con un mensaje descriptivo.

## Ejemplos

### Crear un libro

```bash
curl -i -X POST http://localhost:5080/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Dune",
    "author": "Frank Herbert",
    "genre": "Ciencia ficción",
    "publicationYear": 1965,
    "readingStatus": "Read",
    "rating": 5
  }'
```

Respuesta `201 Created`:

```json
{
  "id": 1,
  "title": "Dune",
  "author": "Frank Herbert",
  "genre": "Ciencia ficción",
  "publicationYear": 1965,
  "readingStatus": "Read",
  "rating": 5,
  "createdAt": "2026-10-20T20:44:40.809396Z"
}
```

### Consultar y actualizar

```bash
# Listar todos los libros
curl http://localhost:5080/api/books

# Obtener un libro por ID
curl http://localhost:5080/api/books/1

# Actualizar todos los datos editables
curl -i -X PUT http://localhost:5080/api/books/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Dune",
    "author": "Frank Herbert",
    "genre": "Ciencia ficción",
    "publicationYear": 1965,
    "readingStatus": "Reading",
    "rating": 4
  }'
```

### Eliminar un libro

```bash
curl -i -X DELETE http://localhost:5080/api/books/1
```

Respuesta `204 No Content`. Si el ID no existe:

```json
{
  "title": "Libro no encontrado",
  "status": 404,
  "detail": "No existe ningún libro con el ID 1.",
  "instance": "/api/books/1"
}
```

### Error de validación

```bash
curl -i -X POST http://localhost:5080/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Libro sin autor",
    "publicationYear": 3000,
    "readingStatus": "Finished"
  }'
```

Respuesta `400 Bad Request`:

```json
{
  "title": "Datos de entrada no válidos",
  "status": 400,
  "instance": "/api/books",
  "errors": {
    "Author": ["El autor es obligatorio."],
    "PublicationYear": ["El año de publicación debe estar entre 1 y 2026."],
    "ReadingStatus": ["El valor del campo 'readingStatus' no es válido."]
  }
}
```

> El año máximo del mensaje se calcula con el año actual del servidor.

## Modelo de datos

| Campo | Tipo | Obligatorio | Reglas |
|---|---|---|---|
| `id` | entero | Sí (generado) | Clave primaria autoincremental |
| `title` | texto | Sí | Máximo 200 caracteres |
| `author` | texto | Sí | Máximo 150 caracteres |
| `genre` | texto | No | Máximo 100 caracteres |
| `publicationYear` | entero | Sí | Entre 1 y el año actual |
| `readingStatus` | texto | Sí | `Pending`, `Reading` o `Read` |
| `rating` | entero | No | Entre 1 y 5 |
| `createdAt` | fecha/hora | Sí (generado) | Se asigna en el servidor, en UTC |

## Cómo probar

1. Ejecuta `dotnet run --project src/BookShelf.Api`.
2. Abre la interfaz web en <http://localhost:5080/> para crear, editar y eliminar libros.
3. Abre <http://localhost:5080/swagger> para probar la API directamente.
4. También puedes usar `src/BookShelf.Api/BookShelf.http` desde Visual Studio o VS Code con la extensión REST Client.

Comandos útiles:

```bash
# Compilar
dotnet build

# Ver las migraciones
dotnet ef migrations list --project src/BookShelf.Api

# Crear una migración después de cambiar el modelo
dotnet ef migrations add NombreDeLaMigracion \
  --project src/BookShelf.Api \
  --startup-project src/BookShelf.Api \
  --output-dir Data/Migrations

# Revertir a una migración anterior
dotnet ef database update NombreMigracionAnterior \
  --project src/BookShelf.Api \
  --startup-project src/BookShelf.Api
```

## Notas técnicas

- **DTOs en lugar de entidades.** La entidad `Book` es un detalle de persistencia; los DTOs controlan qué se recibe y qué se devuelve.
- **Enum como texto.** La columna `reading_status` guarda `Pending`, `Reading` o `Read`, y `JsonStringEnumConverter` rechaza valores numéricos o desconocidos.
- **Fechas en UTC.** `CreatedAt` se genera con `DateTime.UtcNow`; el mapeo marca el valor como UTC al leerlo.
- **Validación en los DTOs.** `DataAnnotations` cubre las reglas simples e `IValidatableObject` valida que el año no sea futuro.
- **Errores homogéneos.** Los `400` se normalizan en `Program.cs` y los `404` usan `ProblemDetails`.
- **Sin capas innecesarias.** No hay repositorio genérico, AutoMapper ni interfaz de servicio; el proyecto es pequeño y cada abstracción tendría que justificar su coste.
- **Credenciales fuera del repositorio.** La cadena de conexión se obtiene de user-secrets o de variables de entorno.

## Roadmap

### v2 — Búsqueda y estadísticas

- Buscar por título o autor.
- Filtrar por género y estado de lectura.
- Combinar filtros.
- Estadísticas: total, pendientes, en lectura, leídos y distribución por género.
- Añadir índices para las columnas usadas en los filtros.
- Mostrar los filtros y las estadísticas en la interfaz web.

### v3 — Calidad

- Pruebas unitarias y de integración.
- Paginación en los listados.
- CI con GitHub Actions.
- Documentación y capturas adicionales.

### Mejoras futuras

- Autenticación y autorización por usuario.
- Interfaz web más completa.
- Docker Compose para levantar API y MySQL.
- Exportación de datos y búsqueda avanzada.

## Autor

Santiago Burgos ([@sburgos28](https://github.com/sburgos28)).
