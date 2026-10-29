using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class DimensionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DimensionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dimension
        public async Task<IActionResult> Index()
        {
            var dimensiones = await _context.Dimensiones
                .OrderBy(d => d.Codigo)
                .ToListAsync();

            return View(dimensiones);
        }

        // GET: Dimension/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
                return NotFound();

            var dimension = await _context.Dimensiones
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dimension == null)
                return NotFound();

            return View(dimension);
        }

        // GET: Dimension/Crear
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Dimension/Crear
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string codigo,
            string nombre,
            string? descripcion)
        {
            codigo = codigo?.Trim().ToUpperInvariant() ?? "";
            nombre = nombre?.Trim() ?? "";
            descripcion = descripcion?.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
                ModelState.AddModelError(
                    "Codigo", "El código es obligatorio.");

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre", "El nombre es obligatorio.");

            if (await _context.Dimensiones.AnyAsync(
                d => d.Codigo.ToUpper() == codigo))
            {
                ModelState.AddModelError(
                    "Codigo", "Ya existe una dimensión con ese código.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Codigo"] = codigo;
                ViewData["Nombre"] = nombre;
                ViewData["Descripcion"] = descripcion;
                return View();
            }

            var dimension = new Dimension
            {
                Codigo = codigo,
                Nombre = nombre,
                Descripcion = descripcion
            };

            _context.Dimensiones.Add(dimension);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La dimensión se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Dimension/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var dimension = await _context.Dimensiones.FindAsync(id);

            if (dimension == null)
                return NotFound();

            return View(dimension);
        }

        // POST: Dimension/Editar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string codigo,
            string nombre,
            string? descripcion)
        {
            codigo = codigo?.Trim().ToUpperInvariant() ?? "";
            nombre = nombre?.Trim() ?? "";
            descripcion = descripcion?.Trim();

            var dimension = await _context.Dimensiones.FindAsync(id);

            if (dimension == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(codigo))
                ModelState.AddModelError(
                    "Codigo", "El código es obligatorio.");

            if (string.IsNullOrWhiteSpace(nombre))
                ModelState.AddModelError(
                    "Nombre", "El nombre es obligatorio.");

            if (await _context.Dimensiones.AnyAsync(
                d => d.Id != id &&
                     d.Codigo.ToUpper() == codigo))
            {
                ModelState.AddModelError(
                    "Codigo", "Otra dimensión ya utiliza ese código.");
            }

            if (!ModelState.IsValid)
            {
                var modelo = new Dimension
                {
                    Id = id,
                    Codigo = codigo,
                    Nombre = nombre,
                    Descripcion = descripcion
                };

                return View(modelo);
            }

            dimension.Codigo = codigo;
            dimension.Nombre = nombre;
            dimension.Descripcion = descripcion;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La dimensión se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Dimension/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var dimension = await _context.Dimensiones
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dimension == null)
                return NotFound();

            return View(dimension);
        }

        // POST: Dimension/Eliminar/5
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var dimension = await _context.Dimensiones.FindAsync(id);

            if (dimension == null)
                return NotFound();

            _context.Dimensiones.Remove(dimension);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "No se puede eliminar la dimensión porque tiene preguntas relacionadas.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Mensaje"] =
                "La dimensión se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
