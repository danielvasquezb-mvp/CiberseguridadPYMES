using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class FormularioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FormularioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Formulario
        public async Task<IActionResult> Index()
        {
            var formularios = await _context.Formularios
                .Include(f => f.TipoSistema)
                .OrderBy(f => f.Nombre)
                .ToListAsync();

            return View(formularios);
        }

        // GET: Formulario/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
                return NotFound();

            var formulario = await _context.Formularios
                .Include(f => f.TipoSistema)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (formulario == null)
                return NotFound();

            return View(formulario);
        }

        // GET: Formulario/Crear
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarTiposSistema();
            return View();
        }

        // POST: Formulario/Crear
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string nombre,
            string version,
            bool activo,
            int tipoSistemaId)
        {
            nombre = nombre?.Trim() ?? "";
            version = version?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre", "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(version))
                ModelState.AddModelError(
                    "Version", "La versión es obligatoria.");

            var tipoExiste = await _context.TiposSistema
                .AnyAsync(t => t.Id == tipoSistemaId);

            if (!tipoExiste)
                ModelState.AddModelError(
                    "TipoSistemaId", "Selecciona un tipo de sistema válido.");

            if (!ModelState.IsValid)
            {
                ViewData["Nombre"] = nombre;
                ViewData["Version"] = version;
                ViewData["Activo"] = activo;
                ViewData["TipoSistemaId"] = tipoSistemaId;

                await CargarTiposSistema(tipoSistemaId);
                return View();
            }

            var formulario = new Formulario
            {
                Nombre = nombre,
                Version = version,
                Activo = activo,
                TipoSistemaId = tipoSistemaId
            };

            _context.Formularios.Add(formulario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El formulario se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Formulario/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var formulario = await _context.Formularios
                .FindAsync(id);

            if (formulario == null)
                return NotFound();

            await CargarTiposSistema(formulario.TipoSistemaId);

            return View(formulario);
        }

        // POST: Formulario/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string version,
            bool activo,
            int tipoSistemaId)
        {
            var formulario = await _context.Formularios.FindAsync(id);

            if (formulario == null)
                return NotFound();

            nombre = nombre?.Trim() ?? "";
            version = version?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre", "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(version))
                ModelState.AddModelError(
                    "Version", "La versión es obligatoria.");

            var tipoExiste = await _context.TiposSistema
                .AnyAsync(t => t.Id == tipoSistemaId);

            if (!tipoExiste)
                ModelState.AddModelError(
                    "TipoSistemaId", "Selecciona un tipo de sistema válido.");

            if (!ModelState.IsValid)
            {
                var modelo = new Formulario
                {
                    Id = id,
                    Nombre = nombre,
                    Version = version,
                    Activo = activo,
                    TipoSistemaId = tipoSistemaId
                };

                await CargarTiposSistema(tipoSistemaId);
                return View(modelo);
            }

            formulario.Nombre = nombre;
            formulario.Version = version;
            formulario.Activo = activo;
            formulario.TipoSistemaId = tipoSistemaId;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El formulario se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Formulario/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var formulario = await _context.Formularios
                .Include(f => f.TipoSistema)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (formulario == null)
                return NotFound();

            return View(formulario);
        }

        // POST: Formulario/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var formulario = await _context.Formularios
                .FindAsync(id);

            if (formulario == null)
                return NotFound();

            _context.Formularios.Remove(formulario);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar el formulario porque tiene preguntas o evaluaciones relacionadas.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] =
                "El formulario se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // Cargar los tipos de sistemas en el selector.
        private async Task CargarTiposSistema(int? seleccionado = null)
        {
            var tipos = await _context.TiposSistema
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            ViewBag.TiposSistema = new SelectList(
                tipos,
                "Id",
                "Nombre",
                seleccionado);
        }
    }
}
