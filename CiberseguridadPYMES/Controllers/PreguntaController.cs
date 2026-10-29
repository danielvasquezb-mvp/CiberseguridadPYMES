
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
            string tipoRespuesta)
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
                TipoRespuesta = tipoRespuesta
            };

            _context.Preguntas.Add(pregunta);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La pregunta se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Pregunta/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var pregunta = await _context.Preguntas.FindAsync(id);

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
            string tipoRespuesta)
        {
            var pregunta = await _context.Preguntas.FindAsync(id);

            if (pregunta == null)
                return NotFound();

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
                    TipoRespuesta = tipoRespuesta
                };

                await CargarListas(formularioId, dimensionId);
                return View(modelo);
            }

            pregunta.FormularioId = formularioId;
            pregunta.DimensionId = dimensionId;
            pregunta.Texto = texto;
            pregunta.ReferenciaTecnica = referenciaTecnica;
            pregunta.CriterioCumplimiento = criterioCumplimiento;
            pregunta.Activa = activa;
            pregunta.Orden = orden;
            pregunta.TipoRespuesta = tipoRespuesta;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La pregunta se actualizó correctamente.";

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
            var pregunta = await _context.Preguntas.FindAsync(id);

            if (pregunta == null)
                return NotFound();

            _context.Preguntas.Remove(pregunta);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar la pregunta porque tiene respuestas o recomendaciones relacionadas.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] =
                "La pregunta se eliminó correctamente.";

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
