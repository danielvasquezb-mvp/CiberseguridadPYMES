using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize(Roles = "Responsable del sistema")]
    public class DiagnosticoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Diagnostico/Detalles?evaluacionId=1
        [HttpGet]
        public async Task<IActionResult> Detalles(int evaluacionId)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var diagnostico = await _context.Diagnosticos
                .Include(d => d.Evaluacion)
                    .ThenInclude(e => e!.Empresa)
                .Include(d => d.Evaluacion)
                    .ThenInclude(e => e!.Formulario)
                .Include(d => d.ResultadosDimension)
                    .ThenInclude(r => r.Dimension)
                .Include(d => d.Hallazgos)
                    .ThenInclude(h => h.Pregunta)
                        .ThenInclude(p => p!.Dimension)
                .FirstOrDefaultAsync(d =>
                    d.EvaluacionId == evaluacionId);

            if (diagnostico == null ||
                diagnostico.Evaluacion == null ||
                diagnostico.Evaluacion.Empresa == null ||
                diagnostico.Evaluacion.Empresa.ApplicationUserId != usuarioId)
            {
                return NotFound();
            }

            return View(diagnostico);
        }

        // POST: Diagnostico/Generar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(int evaluacionId)
        {
            string? usuarioId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Empresa)
                .Include(e => e.Respuestas)
                .FirstOrDefaultAsync(e => e.Id == evaluacionId);

            if (evaluacion == null ||
                evaluacion.Empresa == null ||
                evaluacion.Empresa.ApplicationUserId != usuarioId)
            {
                return NotFound();
            }

            if (evaluacion.Estado != "Finalizada")
            {
                TempData["Error"] =
                    "Debes finalizar la evaluación antes de generar el diagnóstico.";

                return RedirectToAction("Index", "Evaluacion");
            }

            // Evitar generar más de un diagnóstico por evaluación.
            bool existeDiagnostico = await _context.Diagnosticos
                .AnyAsync(d => d.EvaluacionId == evaluacionId);

            if (existeDiagnostico)
            {
                return RedirectToAction(
                    nameof(Detalles),
                    new { evaluacionId });
            }

            var preguntas = await _context.Preguntas
                .Include(p => p.Dimension)
                .Where(p =>
                    p.FormularioId == evaluacion.FormularioId &&
                    p.Activa)
                .OrderBy(p => p.Orden)
                .ToListAsync();

            if (preguntas.Count == 0)
            {
                TempData["Error"] =
                    "No hay preguntas activas para generar el diagnóstico.";

                return RedirectToAction("Index", "Evaluacion");
            }

            if (preguntas.Any(p => p.Dimension == null))
            {
                TempData["Error"] =
                    "Hay preguntas sin dimensión asignada. Revisa la configuración.";

                return RedirectToAction("Index", "Evaluacion");
            }

            var respuestas = evaluacion.Respuestas
                .ToDictionary(r => r.PreguntaId, r => r);

            // Validar que todas las preguntas activas estén respondidas
            // y que los valores correspondan al tipo de pregunta.
            foreach (var pregunta in preguntas)
            {
                if (!respuestas.TryGetValue(
                    pregunta.Id, out var respuesta))
                {
                    TempData["Error"] =
                        "La evaluación no tiene todas las respuestas requeridas.";

                    return RedirectToAction("Index", "Evaluacion");
                }

                bool respuestaValida = pregunta.TipoRespuesta switch
                {
                    "DosOpciones" =>
                        respuesta.Valor == 1 ||
                        respuesta.Valor == 3,

                    "CincoOpciones" =>
                        respuesta.Valor >= 1 &&
                        respuesta.Valor <= 5,

                    _ => false
                };

                if (!respuestaValida)
                {
                    TempData["Error"] =
                        $"La respuesta de la pregunta {pregunta.Id} no es válida.";

                    return RedirectToAction("Index", "Evaluacion");
                }
            }

            // Excluir No aplica (5) del cálculo del cumplimiento.
            var preguntasAplicables = preguntas
                .Where(p => respuestas[p.Id].Valor != 5)
                .ToList();

            int totalAplicables = preguntasAplicables.Count;

            int totalCumplidas = preguntasAplicables.Count(
                p => respuestas[p.Id].Valor == 1);

            decimal porcentajeGeneral = totalAplicables == 0
                ? 0
                : Math.Round(
                    totalCumplidas * 100m / totalAplicables, 2);

            string resumen = totalAplicables == 0
                ? "No fue posible calcular el cumplimiento porque todas las preguntas se marcaron como No aplica."
                : $"Cumplimiento general: {porcentajeGeneral}%. " +
                  $"Preguntas aplicables: {totalAplicables}. " +
                  $"Preguntas cumplidas: {totalCumplidas}.";

            var diagnostico = new Diagnostico
            {
                EvaluacionId = evaluacionId,
                FechaGeneracion = DateTime.Now,
                PorcentajeGeneral = porcentajeGeneral,
                Resumen = resumen
            };

            // Calcular los resultados de cada dimensión.
            foreach (var grupo in preguntas.GroupBy(
                p => p.DimensionId))
            {
                var aplicablesDimension = grupo
                    .Where(p => respuestas[p.Id].Valor != 5)
                    .ToList();

                int cumplidas = aplicablesDimension.Count(
                    p => respuestas[p.Id].Valor == 1);

                decimal porcentajeDimension =
                    aplicablesDimension.Count == 0
                        ? 0
                        : Math.Round(
                            cumplidas * 100m /
                            aplicablesDimension.Count, 2);

                diagnostico.ResultadosDimension.Add(
                    new ResultadoDimension
                    {
                        DimensionId = grupo.Key,
                        PreguntasAplicables =
                            aplicablesDimension.Count,
                        PreguntasCumplidas = cumplidas,
                        PorcentajeCumplimiento =
                            porcentajeDimension
                    });
            }

            // Crear hallazgos para las respuestas que requieren atención.
            foreach (var pregunta in preguntas)
            {
                var respuesta = respuestas[pregunta.Id];

                // Sí y No aplica no generan hallazgos.
                if (respuesta.Valor == 1 ||
                    respuesta.Valor == 5)
                {
                    continue;
                }

                string descripcion = respuesta.Valor switch
                {
                    2 => "La práctica se cumple parcialmente: " +
                         pregunta.Texto,

                    3 => "La práctica no se cumple: " +
                         pregunta.Texto,

                    4 => "El responsable no conoce si se cumple la práctica: " +
                         pregunta.Texto,

                    _ => "Se requiere revisar la respuesta: " +
                         pregunta.Texto
                };

                string prioridad = respuesta.Valor switch
                {
                    3 => "Alta",
                    2 => "Media",
                    4 => "Media",
                    _ => "Baja"
                };

                diagnostico.Hallazgos.Add(new Hallazgo
                {
                    PreguntaId = pregunta.Id,
                    Descripcion = descripcion,
                    Prioridad = prioridad
                });
            }

            _context.Diagnosticos.Add(diagnostico);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El diagnóstico se generó correctamente.";

            return RedirectToAction(
                nameof(Detalles),
                new { evaluacionId });
        }
    }
}