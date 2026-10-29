using CiberseguridadPYMES.Data;
using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CiberseguridadPYMES.Controllers
{
    [Authorize]
    public class EmpresaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmpresaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Empresa
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresas = await _context.Empresas
                .Where(e => e.ApplicationUserId == usuarioId)
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            return View(empresas);
        }

        // GET: Empresa/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Empresa/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Empresa empresa)
        {
            // El usuario propietario se obtiene de la sesión,
            // no de los datos enviados desde el formulario.
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(usuarioId))
                return Challenge();

            if (string.IsNullOrWhiteSpace(empresa.Nombre))
                ModelState.AddModelError("Nombre", "El nombre de la empresa es obligatorio.");

            if (string.IsNullOrWhiteSpace(empresa.Sector))
                ModelState.AddModelError("Sector", "El sector es obligatorio.");

            if (string.IsNullOrWhiteSpace(empresa.Municipio))
                ModelState.AddModelError("Municipio", "El municipio es obligatorio.");

            if (!ModelState.IsValid)
                return View(empresa);

            var nuevaEmpresa = new Empresa
            {
                Nombre = empresa.Nombre.Trim(),
                Sector = empresa.Sector.Trim(),
                Municipio = empresa.Municipio.Trim(),
                ApplicationUserId = usuarioId
            };

            _context.Empresas.Add(nuevaEmpresa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La empresa se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Empresa/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
                return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ApplicationUserId == usuarioId);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        // GET: Empresa/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
                return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ApplicationUserId == usuarioId);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        // POST: Empresa/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Empresa empresa)
        {
            if (id != empresa.Id)
                return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresaExistente = await _context.Empresas
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ApplicationUserId == usuarioId);

            if (empresaExistente == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(empresa.Nombre))
                ModelState.AddModelError("Nombre", "El nombre de la empresa es obligatorio.");

            if (string.IsNullOrWhiteSpace(empresa.Sector))
                ModelState.AddModelError("Sector", "El sector es obligatorio.");

            if (string.IsNullOrWhiteSpace(empresa.Municipio))
                ModelState.AddModelError("Municipio", "El municipio es obligatorio.");

            if (!ModelState.IsValid)
                return View(empresa);

            // Solo se modifican los campos permitidos.
            empresaExistente.Nombre = empresa.Nombre.Trim();
            empresaExistente.Sector = empresa.Sector.Trim();
            empresaExistente.Municipio = empresa.Municipio.Trim();

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Los datos de la empresa se actualizaron correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Empresa/Eliminar/5
        [HttpGet]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
                return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ApplicationUserId == usuarioId);

            if (empresa == null)
                return NotFound();

            return View(empresa);
        }

        // POST: Empresa/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var empresa = await _context.Empresas
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.ApplicationUserId == usuarioId);

            if (empresa == null)
                return NotFound();

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La empresa se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}