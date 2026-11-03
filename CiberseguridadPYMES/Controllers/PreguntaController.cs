
using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class PreguntaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PreguntaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pregunta
        public async Task<IActionResult> Index()
        {
            var preguntas = await _context.Preguntas
                .Include(p => p.Formulario)
                .Include(p => p.Dimension)
                .OrderBy(p => p.Formulario!.Nombre)
                .ThenBy(p => p.Orden)
                .ToListAsync();

            return View(preguntas);
        }

        // GET: Pregunta/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
                return NotFound();

            var pregunta = await _context.Preguntas
                .Include(p => p.Formulario)
                .Include(p => p.Dimension)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            return View(pregunta);
        }

        // GET: Pregunta/Crear
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarListas();
            return View();
        }

        // POST: Pregunta/Crear

        // POST: Pregunta/Crear
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int formularioId,
            int dimensionId,
            string texto,
            string? referenciaTecnica,
            string? criterioCumplimiento,
            bool activa,
            int orden,
            string tipoRespuesta,
            List<Recomendacion>? recomendaciones)
        {
            texto = texto?.Trim() ?? string.Empty;
            referenciaTecnica = referenciaTecnica?.Trim();
            criterioCumplimiento = criterioCumplimiento?.Trim();
            tipoRespuesta = tipoRespuesta?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
            {
                ModelState.AddModelError(
                    "Texto",
                    "El texto de la pregunta es obligatorio.");
            }

            if (orden < 1)
            {
                ModelState.AddModelError(
                    "Orden",
                    "El orden debe ser mayor que cero.");
            }

            if (tipoRespuesta != "DosOpciones" &&
                tipoRespuesta != "CincoOpciones")
            {
                ModelState.AddModelError(
                    "TipoRespuesta",
                    "Selecciona un tipo de respuesta válido.");
            }

            bool formularioExiste = await _context.Formularios
                .AnyAsync(f => f.Id == formularioId);

            if (!formularioExiste)
            {
                ModelState.AddModelError(
                    "FormularioId",
                    "Selecciona un formulario válido.");
            }

            bool dimensionExiste = await _context.Dimensiones
                .AnyAsync(d => d.Id == dimensionId);

            if (!dimensionExiste)
            {
                ModelState.AddModelError(
                    "DimensionId",
                    "Selecciona una dimensión válida.");
            }

            // Limpiar y validar las recomendaciones recibidas.
            var recomendacionesValidas = new List<Recomendacion>();

            if (recomendaciones != null)
            {
                for (int i = 0; i < recomendaciones.Count; i++)
                {
                    var recomendacion = recomendaciones[i];

                    string titulo = recomendacion.Titulo?.Trim() ?? "";
                    string descripcion = recomendacion.Descripcion?.Trim() ?? "";
                    string prioridad = recomendacion.Prioridad?.Trim() ?? "";

                    // Ignorar filas completamente vacías.
                    if (string.IsNullOrWhiteSpace(titulo) &&
                        string.IsNullOrWhiteSpace(descripcion))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(titulo))
                    {
                        ModelState.AddModelError(
                            $"recomendaciones[{i}].Titulo",
                            "El título de la recomendación es obligatorio.");
                    }

                    if (string.IsNullOrWhiteSpace(descripcion))
                    {
                        ModelState.AddModelError(
                            $"recomendaciones[{i}].Descripcion",
                            "La descripción de la recomendación es obligatoria.");
                    }

                    if (prioridad != "Alta" &&
                        prioridad != "Media" &&
                        prioridad != "Baja")
                    {
                        ModelState.AddModelError(
                            $"recomendaciones[{i}].Prioridad",
                            "Selecciona una prioridad válida.");
                    }

                    recomendacionesValidas.Add(new Recomendacion
                    {
                        Titulo = titulo,
                        Descripcion = descripcion,
                        Prioridad = prioridad,
                        Activa = true,
                        HallazgoId = null
                    });
                }
            }

            if (!ModelState.IsValid)
            {
                ViewData["FormularioId"] = formularioId;
                ViewData["DimensionId"] = dimensionId;
                ViewData["Texto"] = texto;
                ViewData["ReferenciaTecnica"] = referenciaTecnica;
                ViewData["CriterioCumplimiento"] = criterioCumplimiento;
                ViewData["Activa"] = activa;
                ViewData["Orden"] = orden;
                ViewData["TipoRespuesta"] = tipoRespuesta;
                ViewData["Recomendaciones"] = recomendaciones ?? new List<Recomendacion>();

                await CargarListas(formularioId, dimensionId);

                return View();
            }

            var pregunta = new Pregunta
            {
                FormularioId = formularioId,
                DimensionId = dimensionId,
                Texto = texto,
                ReferenciaTecnica = referenciaTecnica,
                CriterioCumplimiento = criterioCumplimiento,
                Activa = activa,
                Orden = orden,
                TipoRespuesta = tipoRespuesta,
                Recomendaciones = recomendacionesValidas
            };

            _context.Preguntas.Add(pregunta);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La pregunta y sus recomendaciones se registraron correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Pregunta/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var pregunta = await _context.Preguntas
                .Include(p => p.Recomendaciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            await CargarListas(
                pregunta.FormularioId,
                pregunta.DimensionId);

            return View(pregunta);
        }

        // POST: Pregunta/Editar/5

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            int formularioId,
            int dimensionId,
            string texto,
            string? referenciaTecnica,
            string? criterioCumplimiento,
            bool activa,
            int orden,
            string tipoRespuesta,
            List<Recomendacion>? recomendaciones)
        {
            var pregunta = await _context.Preguntas
                .Include(p => p.Recomendaciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            texto = texto?.Trim() ?? string.Empty;
            referenciaTecnica = referenciaTecnica?.Trim();
            criterioCumplimiento = criterioCumplimiento?.Trim();
            tipoRespuesta = tipoRespuesta?.Trim() ?? string.Empty;

            recomendaciones ??= new List<Recomendacion>();

            // Validar los datos de la pregunta.

            if (string.IsNullOrWhiteSpace(texto))
            {
                ModelState.AddModelError(
                    "Texto",
                    "El texto de la pregunta es obligatorio.");
            }

            if (orden < 1)
            {
                ModelState.AddModelError(
                    "Orden",
                    "El orden debe ser mayor que cero.");
            }

            if (tipoRespuesta != "DosOpciones" &&
                tipoRespuesta != "CincoOpciones")
            {
                ModelState.AddModelError(
                    "TipoRespuesta",
                    "Selecciona un tipo de respuesta válido.");
            }

            bool formularioExiste = await _context.Formularios
                .AnyAsync(f => f.Id == formularioId);

            if (!formularioExiste)
            {
                ModelState.AddModelError(
                    "FormularioId",
                    "Selecciona un formulario válido.");
            }

            bool dimensionExiste = await _context.Dimensiones
                .AnyAsync(d => d.Id == dimensionId);

            if (!dimensionExiste)
            {
                ModelState.AddModelError(
                    "DimensionId",
                    "Selecciona una dimensión válida.");
            }

            // Validar las recomendaciones recibidas.

            var recomendacionesValidas = recomendaciones
                .Where(r =>
                    !string.IsNullOrWhiteSpace(r.Titulo) ||
                    !string.IsNullOrWhiteSpace(r.Descripcion))
                .ToList();

            var idsRecibidos = recomendacionesValidas
                .Where(r => r.Id > 0)
                .Select(r => r.Id)
                .ToList();

            if (idsRecibidos.Distinct().Count() != idsRecibidos.Count)
            {
                ModelState.AddModelError(
                    "Recomendaciones",
                    "Hay recomendaciones duplicadas.");
            }

            foreach (var recomendacion in recomendacionesValidas)
            {
                recomendacion.Titulo = recomendacion.Titulo?.Trim() ?? string.Empty;
                recomendacion.Descripcion = recomendacion.Descripcion?.Trim() ?? string.Empty;
                recomendacion.Prioridad = recomendacion.Prioridad?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(recomendacion.Titulo))
                {
                    ModelState.AddModelError(
                        "Recomendaciones",
                        "Todas las recomendaciones deben tener un título.");
                }

                if (string.IsNullOrWhiteSpace(recomendacion.Descripcion))
                {
                    ModelState.AddModelError(
                        "Recomendaciones",
                        "Todas las recomendaciones deben tener una descripción.");
                }

                if (recomendacion.Prioridad != "Alta" &&
                    recomendacion.Prioridad != "Media" &&
                    recomendacion.Prioridad != "Baja")
                {
                    ModelState.AddModelError(
                        "Recomendaciones",
                        "Selecciona una prioridad válida para cada recomendación.");
                }

                // Comprobar que los identificadores pertenezcan a esta pregunta.

                if (recomendacion.Id > 0 &&
                    !pregunta.Recomendaciones.Any(r => r.Id == recomendacion.Id))
                {
                    ModelState.AddModelError(
                        "Recomendaciones",
                        "Una de las recomendaciones no pertenece a esta pregunta.");
                }
            }

            // Si hay errores, devolver el formulario con los datos ingresados.

            if (!ModelState.IsValid)
            {
                var modelo = new Pregunta
                {
                    Id = id,
                    FormularioId = formularioId,
                    DimensionId = dimensionId,
                    Texto = texto,
                    ReferenciaTecnica = referenciaTecnica,
                    CriterioCumplimiento = criterioCumplimiento,
                    Activa = activa,
                    Orden = orden,
                    TipoRespuesta = tipoRespuesta,
                    Recomendaciones = recomendaciones
                };

                await CargarListas(formularioId, dimensionId);

                return View(modelo);
            }

            // Actualizar los datos principales de la pregunta.

            pregunta.FormularioId = formularioId;
            pregunta.DimensionId = dimensionId;
            pregunta.Texto = texto;
            pregunta.ReferenciaTecnica = referenciaTecnica;
            pregunta.CriterioCumplimiento = criterioCumplimiento;
            pregunta.Activa = activa;
            pregunta.Orden = orden;
            pregunta.TipoRespuesta = tipoRespuesta;

            // Identificar las recomendaciones que permanecen.

            var idsConservados = recomendacionesValidas
                .Where(r => r.Id > 0)
                .Select(r => r.Id)
                .ToHashSet();

            // Eliminar las recomendaciones que fueron retiradas del formulario.

            var recomendacionesEliminar = pregunta.Recomendaciones
                .Where(r => !idsConservados.Contains(r.Id))
                .ToList();

            _context.Recomendaciones.RemoveRange(recomendacionesEliminar);

            // Actualizar las existentes y agregar las nuevas.

            foreach (var recomendacion in recomendacionesValidas)
            {
                if (recomendacion.Id > 0)
                {
                    var existente = pregunta.Recomendaciones
                        .First(r => r.Id == recomendacion.Id);

                    existente.Titulo = recomendacion.Titulo;
                    existente.Descripcion = recomendacion.Descripcion;
                    existente.Prioridad = recomendacion.Prioridad;
                    existente.Activa = true;
                    existente.HallazgoId = null;
                }
                else
                {
                    pregunta.Recomendaciones.Add(new Recomendacion
                    {
                        Titulo = recomendacion.Titulo,
                        Descripcion = recomendacion.Descripcion,
                        Prioridad = recomendacion.Prioridad,
                        Activa = true,
                        HallazgoId = null
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La pregunta y sus recomendaciones se actualizaron correctamente.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Pregunta/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var pregunta = await _context.Preguntas
                .Include(p => p.Formulario)
                .Include(p => p.Dimension)
                .Include(p => p.Recomendaciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            return View(pregunta);
        }

        // POST: Pregunta/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var pregunta = await _context.Preguntas
                .Include(p => p.Recomendaciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null)
                return NotFound();

            // Verificar si existen respuestas asociadas.
            bool tieneRespuestas = await _context.Respuestas
                .AnyAsync(r => r.PreguntaId == id);

            if (tieneRespuestas)
            {
                TempData["Error"] =
                    "No se puede eliminar esta pregunta porque tiene respuestas registradas en evaluaciones.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Eliminar las recomendaciones asociadas.
                _context.Recomendaciones.RemoveRange(
                    pregunta.Recomendaciones);

                // Eliminar la pregunta.
                _context.Preguntas.Remove(pregunta);

                await _context.SaveChangesAsync();

                TempData["Mensaje"] =
                    "La pregunta y sus recomendaciones se eliminaron correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar la pregunta porque existen otros registros relacionados.";

            }

            return RedirectToAction(nameof(Index));
        }

        // Cargar formularios y dimensiones en los selectores.
        private async Task CargarListas(
            int? formularioSeleccionado = null,
            int? dimensionSeleccionada = null)
        {
            var formularios = await _context.Formularios
                .Where(f => f.Activo)
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            var dimensiones = await _context.Dimensiones
                .OrderBy(d => d.Nombre)
                .ToListAsync();

            ViewBag.Formularios = new SelectList(
                formularios,
                "Id",
                "Nombre",
                formularioSeleccionado);

            ViewBag.Dimensiones = new SelectList(
                dimensiones,
                "Id",
                "Nombre",
                dimensionSeleccionada);
        }
    }
}
