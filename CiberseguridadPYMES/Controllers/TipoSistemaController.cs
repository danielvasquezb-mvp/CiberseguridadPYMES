using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class TipoSistemaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoSistemaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TipoSistema
        // Los usuarios autenticados pueden consultar los tipos.
        public async Task<IActionResult> Index()
        {
            var tiposSistema = await _context.TiposSistema
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return View(tiposSistema);
        }

        // GET: TipoSistema/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoSistema = await _context.TiposSistema
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipoSistema == null)
                return NotFound();

            return View(tipoSistema);
        }

        // GET: TipoSistema/Crear
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: TipoSistema/Crear
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string nombre,
            string? descripcion)
        {
            nombre = nombre?.Trim() ?? "";
            descripcion = descripcion?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre",
                    "El nombre del tipo de sistema es obligatorio.");

            bool existe = await _context.TiposSistema
                .AnyAsync(t => t.Nombre.ToLower() == nombre.ToLower());

            if (!string.IsNullOrWhiteSpace(nombre) && existe)
                ModelState.AddModelError(
                    "Nombre",
                    "Ya existe un tipo de sistema con ese nombre.");

            if (!ModelState.IsValid)
            {
                ViewData["Nombre"] = nombre;
                ViewData["Descripcion"] = descripcion;
                return View();
            }

            var tipoSistema = new TipoSistema
            {
                Nombre = nombre,
                Descripcion = descripcion
            };

            _context.TiposSistema.Add(tipoSistema);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El tipo de sistema se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: TipoSistema/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoSistema = await _context.TiposSistema
                .FindAsync(id);

            if (tipoSistema == null)
                return NotFound();

            return View(tipoSistema);
        }

        // POST: TipoSistema/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre",
                    "El nombre del tipo de sistema es obligatorio.");

            var tipoSistema = await _context.TiposSistema
                .FindAsync(id);

            if (tipoSistema == null)
                return NotFound();

            nombre = nombre?.Trim() ?? "";
            descripcion = descripcion?.Trim();

            bool existe = await _context.TiposSistema
                .AnyAsync(t =>
                    t.Id != id &&
                    t.Nombre.ToLower() == nombre.ToLower());

            if (!string.IsNullOrWhiteSpace(nombre) && existe)
                ModelState.AddModelError(
                    "Nombre",
                    "Ya existe otro tipo de sistema con ese nombre.");

            if (!ModelState.IsValid)
            {
                var modelo = new TipoSistema
                {
                    Id = id,
                    Nombre = nombre,
                    Descripcion = descripcion
                };

                return View(modelo);
            }

            tipoSistema.Nombre = nombre;
            tipoSistema.Descripcion = descripcion;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "El tipo de sistema se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: TipoSistema/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var tipoSistema = await _context.TiposSistema
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipoSistema == null)
                return NotFound();

            return View(tipoSistema);
        }

        // POST: TipoSistema/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var tipoSistema = await _context.TiposSistema
                .FindAsync(id);

            if (tipoSistema == null)
                return NotFound();

            _context.TiposSistema.Remove(tipoSistema);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar este tipo de sistema porque está relacionado con otros registros.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] =
                "El tipo de sistema se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
