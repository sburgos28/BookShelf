// La interfaz se sirve desde la misma API: las rutas relativas evitan CORS.
const API_URL = '/api/books';

// La API usa valores en inglés; aquí se traducen para mostrarlos.
const STATUS_LABELS = {
  Pending: 'Pendiente',
  Reading: 'Leyendo',
  Read: 'Leído'
};

const ERROR_FIELD_LABELS = {
  Title: 'Título',
  Author: 'Autor',
  Genre: 'Género',
  PublicationYear: 'Año',
  ReadingStatus: 'Estado',
  Rating: 'Valoración',
  body: 'Petición'
};

// Referencias a los elementos del DOM que se actualizan desde JavaScript.
const bookForm = document.querySelector('#book-form');
const formTitle = document.querySelector('#form-title');
const submitButton = document.querySelector('#submit-button');
const cancelButton = document.querySelector('#cancel-button');
const messageBox = document.querySelector('#message');
const booksBody = document.querySelector('#books-body');
const bookCount = document.querySelector('#book-count');
const yearInput = bookForm.elements.namedItem('publicationYear');

// Estado en memoria: libros cargados y libro en edición (null = crear).
let books = [];
let editingId = null;
let messageTimeout;

yearInput.max = String(new Date().getFullYear());

bookForm.addEventListener('submit', handleSubmit);
cancelButton.addEventListener('click', resetForm);

loadBooks();

// Carga la lista desde la API y pinta la tabla. Devuelve true si todo fue bien.
async function loadBooks() {
  booksBody.replaceChildren(createMessageRow('Cargando libros…', 'loading-row', 7));

  try {
    const response = await fetch(API_URL, {
      headers: { Accept: 'application/json' }
    });

    if (!response.ok) {
      throw new Error('No se pudo cargar la lista de libros.');
    }

    books = await response.json();
    renderBooks();
    return true;
  } catch (error) {
    books = [];
    renderBooks();
    showMessage(error.message, 'error');
    return false;
  }
}

// Reconstruye la tabla a partir del array books.
function renderBooks() {
  booksBody.replaceChildren();
  bookCount.textContent = `${books.length} ${books.length === 1 ? 'libro' : 'libros'}`;

  if (books.length === 0) {
    booksBody.append(
      createMessageRow('Todavía no hay libros. Añade el primero con el formulario.', 'empty-row', 7)
    );
    return;
  }

  for (const book of books) {
    booksBody.append(createBookRow(book));
  }
}

// Usa textContent (no innerHTML) para evitar inyección de HTML.
function createBookRow(book) {
  const row = document.createElement('tr');

  row.append(createTextCell(book.title, 'title-cell'));
  row.append(createTextCell(book.author));
  row.append(createTextCell(book.genre ?? '—'));
  row.append(createTextCell(String(book.publicationYear)));

  const statusCell = document.createElement('td');
  const badge = document.createElement('span');
  badge.className = `badge badge-${book.readingStatus.toLowerCase()}`;
  badge.textContent = STATUS_LABELS[book.readingStatus] ?? book.readingStatus;
  statusCell.append(badge);
  row.append(statusCell);

  const ratingCell = document.createElement('td');
  ratingCell.className = 'rating';
  if (book.rating) {
    ratingCell.textContent = '★'.repeat(book.rating) + '☆'.repeat(5 - book.rating);
    ratingCell.title = `${book.rating} de 5`;
    ratingCell.setAttribute('aria-label', `Valoración: ${book.rating} de 5`);
  } else {
    ratingCell.textContent = '—';
    ratingCell.setAttribute('aria-label', 'Sin valorar');
  }
  row.append(ratingCell);

  const actionsCell = document.createElement('td');
  actionsCell.className = 'actions-cell';
  actionsCell.append(
    createButton('Editar', 'button-small', () => startEdit(book)),
    createButton('Borrar', 'button-small button-danger', () => deleteBook(book))
  );
  row.append(actionsCell);

  return row;
}

function createTextCell(text, className) {
  const cell = document.createElement('td');
  cell.textContent = text;
  if (className) {
    cell.className = className;
  }
  return cell;
}

function createButton(label, className, onClick) {
  const button = document.createElement('button');
  button.type = 'button';
  button.className = `button ${className}`.trim();
  button.textContent = label;
  button.addEventListener('click', onClick);
  return button;
}

function createMessageRow(text, className, columns) {
  const row = document.createElement('tr');
  row.className = className;

  const cell = document.createElement('td');
  cell.colSpan = columns;
  cell.textContent = text;
  row.append(cell);

  return row;
}

// Rellena el formulario con el libro seleccionado y cambia a modo edición.
function startEdit(book) {
  editingId = book.id;

  bookForm.elements.namedItem('title').value = book.title;
  bookForm.elements.namedItem('author').value = book.author;
  bookForm.elements.namedItem('genre').value = book.genre ?? '';
  bookForm.elements.namedItem('publicationYear').value = String(book.publicationYear);
  bookForm.elements.namedItem('readingStatus').value = book.readingStatus;
  bookForm.elements.namedItem('rating').value = book.rating ? String(book.rating) : '';

  formTitle.textContent = 'Editar libro';
  submitButton.textContent = 'Guardar cambios';
  cancelButton.hidden = false;

  clearMessage();
  document.querySelector('.form-panel').scrollIntoView({ behavior: 'smooth', block: 'start' });
  bookForm.elements.namedItem('title').focus({ preventScroll: true });
}

// Vuelve al modo creación y limpia los campos.
function resetForm() {
  bookForm.reset();
  editingId = null;
  formTitle.textContent = 'Añadir libro';
  submitButton.textContent = 'Añadir libro';
  cancelButton.hidden = true;
  clearMessage();
}

// Valida en cliente para dar feedback rápido; la API vuelve a validar por seguridad.
async function handleSubmit(event) {
  event.preventDefault();
  clearMessage();

  const formData = new FormData(bookForm);
  const title = String(formData.get('title') ?? '').trim();
  const author = String(formData.get('author') ?? '').trim();
  const genre = String(formData.get('genre') ?? '').trim();
  const publicationYear = Number(formData.get('publicationYear'));
  const readingStatus = String(formData.get('readingStatus') ?? '');
  const rating = formData.get('rating');

  if (!title || !author) {
    showMessage('El título y el autor son obligatorios.', 'error');
    return;
  }

  const currentYear = new Date().getFullYear();
  if (!Number.isInteger(publicationYear) || publicationYear < 1 || publicationYear > currentYear) {
    showMessage(`El año de publicación debe estar entre 1 y ${currentYear}.`, 'error');
    return;
  }

  if (!readingStatus) {
    showMessage('Selecciona un estado de lectura.', 'error');
    return;
  }

  const payload = {
    title,
    author,
    genre: genre || null,
    publicationYear,
    readingStatus,
    rating: rating ? Number(rating) : null
  };

  const isEditing = editingId !== null;
  const url = isEditing ? `${API_URL}/${editingId}` : API_URL;
  const method = isEditing ? 'PUT' : 'POST';

  submitButton.disabled = true;

  try {
    const response = await fetch(url, {
      method,
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json'
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      const problem = await response.json().catch(() => null);
      showMessage(formatProblem(problem), 'error');
      return;
    }

    const reloaded = await loadBooks();
    resetForm();
    showMessage(
      reloaded
        ? (isEditing ? 'Libro actualizado correctamente.' : 'Libro añadido correctamente.')
        : 'El libro se guardó, pero no se pudo recargar la lista.',
      reloaded ? 'success' : 'error'
    );
  } catch (error) {
    showMessage('No se pudo conectar con la API. Comprueba que el servidor esté en marcha.', 'error');
  } finally {
    submitButton.disabled = false;
  }
}

// Pide confirmación, borra el libro y recarga la tabla.
async function deleteBook(book) {
  if (!window.confirm(`¿Seguro que quieres eliminar "${book.title}"?`)) {
    return;
  }

  try {
    const response = await fetch(`${API_URL}/${book.id}`, { method: 'DELETE' });

    if (response.status === 204) {
      if (editingId === book.id) {
        resetForm();
      }

      const reloaded = await loadBooks();
      showMessage(
        reloaded ? 'Libro eliminado correctamente.' : 'El libro se eliminó, pero no se pudo recargar la lista.',
        reloaded ? 'success' : 'error'
      );
      return;
    }

    if (response.status === 404) {
      showMessage('Ese libro ya no existe.', 'error');
      await loadBooks();
      return;
    }

    const problem = await response.json().catch(() => null);
    showMessage(formatProblem(problem), 'error');
  } catch (error) {
    showMessage('No se pudo conectar con la API.', 'error');
  }
}

// Convierte el ValidationProblemDetails de la API en un mensaje legible.
function formatProblem(problem) {
  if (problem?.errors && typeof problem.errors === 'object') {
    const messages = [];

    for (const [field, fieldErrors] of Object.entries(problem.errors)) {
      if (!Array.isArray(fieldErrors)) {
        continue;
      }

      const label = ERROR_FIELD_LABELS[field] ?? field;
      for (const error of fieldErrors) {
        messages.push(`${label}: ${error}`);
      }
    }

    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  return problem?.detail || problem?.title || 'No se pudo completar la operación.';
}

// Muestra un mensaje; los de éxito desaparecen solos a los pocos segundos.
function showMessage(text, type) {
  clearTimeout(messageTimeout);

  messageBox.textContent = text;
  messageBox.className = `message message-${type}`;
  messageBox.hidden = false;

  if (type === 'success') {
    messageTimeout = setTimeout(clearMessage, 4500);
  }
}

function clearMessage() {
  clearTimeout(messageTimeout);
  messageBox.textContent = '';
  messageBox.className = 'message';
  messageBox.hidden = true;
}
