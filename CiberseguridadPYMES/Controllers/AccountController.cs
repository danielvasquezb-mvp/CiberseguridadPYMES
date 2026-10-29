using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CiberseguridadPYMES.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Mostrar formulario de registro
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Registrar usuario
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string nombreCompleto,
            string correo,
            string password,
            string confirmarPassword)
        {
            nombreCompleto = nombreCompleto?.Trim() ?? "";
            correo = correo?.Trim() ?? "";
            password ??= "";
            confirmarPassword ??= "";

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                ModelState.AddModelError(
                    "nombreCompleto",
                    "El nombre completo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo) ||
                !new EmailAddressAttribute().IsValid(correo))
            {
                ModelState.AddModelError(
                    "correo",
                    "Ingresa un correo electrónico válido.");
            }

            if (password.Length < 8)
            {
                ModelState.AddModelError(
                    "password",
                    "La contraseña debe tener al menos 8 caracteres.");
            }

            if (password != confirmarPassword)
            {
                ModelState.AddModelError(
                    "confirmarPassword",
                    "Las contraseñas no coinciden.");
            }

            if (!ModelState.IsValid)
                return View();

            var correoExiste = await _userManager.FindByEmailAsync(correo);

            if (correoExiste != null)
            {
                ModelState.AddModelError(
                    "correo",
                    "Este correo electrónico ya está registrado.");

                return View();
            }

            var usuario = new ApplicationUser
            {
                NombreCompleto = nombreCompleto,
                Email = correo,
                UserName = correo
            };

            var resultado = await _userManager.CreateAsync(
                usuario, password);

            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    usuario,
                    "Responsable del sistema");

                await _signInManager.SignInWithClaimsAsync(
                    usuario,
                    isPersistent: false,
                    new[]
                    {
            new Claim("NombreCompleto", usuario.NombreCompleto)
                    });

                TempData["Mensaje"] = "Tu cuenta se creó correctamente.";

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View();
        }

        // Mostrar formulario de inicio de sesión
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // Validar credenciales e iniciar sesión

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string correo,
            string password,
            bool recordarSesion = false,
            string? returnUrl = null)
        {
            correo = correo?.Trim() ?? "";
            password ??= "";

            ViewBag.ReturnUrl = returnUrl;
            ViewData["Correo"] = correo;

            if (string.IsNullOrWhiteSpace(correo) ||
                !new EmailAddressAttribute().IsValid(correo))
            {
                ModelState.AddModelError(
                    "correo",
                    "Ingresa un correo electrónico válido.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "password",
                    "La contraseña es obligatoria.");
            }

            if (!ModelState.IsValid)
                return View();

            var usuario = await _userManager.FindByEmailAsync(correo);

            if (usuario != null)
            {
                var resultado = await _signInManager.CheckPasswordSignInAsync(
                    usuario,
                    password,
                    lockoutOnFailure: false);

                if (resultado.Succeeded)
                {
                    await _signInManager.SignInWithClaimsAsync(
                    usuario,
                    isPersistent: false,
                    new[]
                    {
                        new Claim(
                            "NombreCompleto",
                            usuario.NombreCompleto ?? "")
                    });

                    if (!string.IsNullOrEmpty(returnUrl) && 
                        Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(
                string.Empty,
                "Correo electrónico o contraseña incorrectos, o cuenta no habilitada.");

            return View();
        }


        // Cerrar sesión
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }

        // Mostrar página de acceso denegado
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}