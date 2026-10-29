
using System.Security.Claims;
using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class EvaluacionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EvaluacionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Evaluacion
        public async Task<IActionResult> Index()
        {
            var consulta = _context.Evaluaciones
                .Include(e => e.Empresa)
                .Include(e => e.Formulario)
                .AsQueryable();

            if (!User.IsInRole("Administrador"))
            {
                string? usuarioId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (usuarioId == null)
                    return Forbid();

                consulta = consulta.Where(e =>
                    e.Empresa != null &&
                    e.Empresa.ApplicationUserId == usuarioId);
            }

            var evaluaciones = await consulta
                .OrderByDescending(e => e.FechaInicio)
                .ToListAsync();

            return View(evaluaciones);
        }

        // GET: Evaluacion/Crear
        [Authorize(Roles = "Responsable del sistema")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarListas();
            return View();
        }

        // POST: Evaluacion/Crear
        [Authorize(Roles = "Responsable del sistema")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            int empresaId,
            int formularioId)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (usuarioId == null)
                return Forbid();

            // La empresa debe pertenecer al usuario autenticado.
            bool empresaPropia = await _context.Empresas
                .AnyAsync(e =>
                    e.Id == empresaId &&
                    e.ApplicationUserId == usuarioId);

            if (!empresaPropia)
            {
                ModelState.AddModelError(
                    "empresaId",
                    "Selecciona una empresa registrada a tu nombre.");
            }

            // Solo se permiten formularios activos.
            var formulario = await _context.Formularios
                .FirstOrDefaultAsync(f =>
                    f.Id == formularioId && f.Activo);

            if (formulario == null)
            {
                ModelState.AddModelError(
                    "formularioId",
                    "Selecciona un formulario activo válido.");
            }
            else
            {
                bool tienePreguntas = await _context.Preguntas
                    .AnyAsync(p =>
                        p.FormularioId == formularioId &&
                        p.Activa);

                if (!tienePreguntas)
                {
                    ModelState.AddModelError(
                        "formularioId",
                        "El formulario no tiene preguntas activas.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarListas(empresaId, formularioId);
                return View();
            }

            var evaluacion = new Evaluacion
            {
                EmpresaId = empresaId,
                FormularioId = formularioId,
                FechaInicio = DateTime.Now,
                Estado = "En proceso"
            };

            _context.Evaluaciones.Add(evaluacion);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La evaluación se inició correctamente.";

            return RedirectToAction(
                nameof(Responder),
                new { id = evaluacion.Id });
        }

        // GET: Evaluacion/Responder/5
        [Authorize(Roles = "Responsable del sistema")]
        [HttpGet]
        public async Task<IActionResult> Responder(int? id)
        {
            if (id == null)
                return NotFound();

            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Empresa)
                .Include(e => e.Formulario)
                .Include(e => e.Respuestas)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Empresa != null &&
                    e.Empresa.ApplicationUserId == usuarioId);

            if (evaluacion == null)
                return NotFound();

            if (evaluacion.Estado != "En proceso")
            {
                TempData["Error"] =
                    "Esta evaluación ya no está disponible para responder.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Preguntas = await _context.Preguntas
                .Include(p => p.Dimension)
                .Where(p =>
                    p.FormularioId == evaluacion.FormularioId &&
                    p.Activa)
                .OrderBy(p => p.Orden)
                .ToListAsync();

            return View(evaluacion);
        }

        // POST: Evaluacion/Guardar/5
        [Authorize(Roles = "Responsable del sistema")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(
            int id,
            IFormCollection datos)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Empresa)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Empresa != null &&
                    e.Empresa.ApplicationUserId == usuarioId);

            if (evaluacion == null)
                return NotFound();

            if (evaluacion.Estado != "En proceso")
                return BadRequest(
                    "La evaluación ya no está en proceso.");

            var preguntas = await _context.Preguntas
                .Where(p =>
                    p.FormularioId == evaluacion.FormularioId &&
                    p.Activa)
                .ToListAsync();

            var respuestasExistentes = await _context.Respuestas
                .Where(r => r.EvaluacionId == id)
                .ToListAsync();

            foreach (var pregunta in preguntas)
            {
                string claveValor = $"valor_{pregunta.Id}";
                string claveObservacion =
                    $"observacion_{pregunta.Id}";

                // Si no se envió respuesta, se conserva la anterior.
                if (!datos.ContainsKey(claveValor))
                    continue;

                if (!int.TryParse(
                        datos[claveValor].ToString(),
                        out int valor))
                {
                    TempData["Error"] =
                        "Una de las respuestas no es válida.";

                    return RedirectToAction(
                        nameof(Responder),
                        new { id });
                }

                // Validar según el tipo configurado en la pregunta.
                bool valorValido;

                if (pregunta.TipoRespuesta == "DosOpciones")
                {
                    valorValido = valor == 1 || valor == 3;
                }
                else if (pregunta.TipoRespuesta == "CincoOpciones")
                {
                    valorValido = valor >= 1 && valor <= 5;
                }
                else
                {
                    valorValido = false;
                }

                if (!valorValido)
                {
                    TempData["Error"] =
                        $"La respuesta de la pregunta {pregunta.Orden} " +
                        "no corresponde al tipo de respuesta configurado.";

                    return RedirectToAction(
                        nameof(Responder),
                        new { id });
                }

                string observacion =
                    datos[claveObservacion].ToString().Trim();

                if (observacion.Length > 1000)
                {
                    TempData["Error"] =
                        "Las observaciones no pueden superar 1000 caracteres.";

                    return RedirectToAction(
                        nameof(Responder),
                        new { id });
                }

                var respuesta = respuestasExistentes
                    .FirstOrDefault(r =>
                        r.PreguntaId == pregunta.Id);

                if (respuesta == null)
                {
                    respuesta = new Respuesta
                    {
                        EvaluacionId = id,
                        PreguntaId = pregunta.Id
                    };

                    _context.Respuestas.Add(respuesta);
                    respuestasExistentes.Add(respuesta);
                }

                respuesta.Valor = valor;

                respuesta.Observaciones =
                    string.IsNullOrWhiteSpace(observacion)
                        ? null
                        : observacion;

                respuesta.FechaRegistro = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Las respuestas se guardaron correctamente.";

            return RedirectToAction(
                nameof(Responder),
                new { id });
        }

        // POST: Evaluacion/Finalizar/5
        [Authorize(Roles = "Responsable del sistema")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Empresa)
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.Empresa != null &&
                    e.Empresa.ApplicationUserId == usuarioId);

            if (evaluacion == null)
                return NotFound();

            if (evaluacion.Estado != "En proceso")
                return BadRequest(
                    "La evaluación ya no está en proceso.");

            var preguntasActivas = await _context.Preguntas
                .Where(p =>
                    p.FormularioId == evaluacion.FormularioId &&
                    p.Activa)
                .Select(p => p.Id)
                .ToListAsync();

            var preguntasRespondidas = await _context.Respuestas
                .Where(r =>
                    r.EvaluacionId == id &&
                    preguntasActivas.Contains(r.PreguntaId))
                .Select(r => r.PreguntaId)
                .Distinct()
                .ToListAsync();

            if (preguntasActivas.Count == 0 ||
                preguntasActivas.Except(preguntasRespondidas).Any())
            {
                TempData["Error"] =
                    "Debes responder todas las preguntas activas antes de finalizar.";

                return RedirectToAction(
                    nameof(Responder),
                    new { id });
            }

            evaluacion.Estado = "Finalizada";
            evaluacion.FechaFinalizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La evaluación finalizó correctamente. El diagnóstico estará disponible cuando se implemente su generación.";

            return RedirectToAction(nameof(Index));
        }

        // Cargar empresas propias y formularios activos.
        private async Task CargarListas(
            int? empresaSeleccionada = null,
            int? formularioSeleccionado = null)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var empresas = await _context.Empresas
                .Where(e => e.ApplicationUserId == usuarioId)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            var formularios = await _context.Formularios
                .Where(f => f.Activo)
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            ViewBag.Empresas = new SelectList(
                empresas,
                "Id",
                "Nombre",
                empresaSeleccionada);

            ViewBag.Formularios = new SelectList(
                formularios,
                "Id",
                "Nombre",
                formularioSeleccionado);
        }
    }
}
