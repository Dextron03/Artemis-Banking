using Application.Interfaces;
using Application.ViewModels.Login;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;   
        private readonly UserManager<AppUser> _userManager;       
        private readonly IAuthService _authService;

        public AccountController(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IAuthService authService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToRoleHome();

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userManager.FindByNameAsync(vm.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(vm);
            }

            if (!user.IsActive || (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.Now))
            {
                ModelState.AddModelError(string.Empty,
                    "Tu cuenta está inactiva. Debes activarla mediante el enlace " +
                    "enviado a tu correo electrónico para poder acceder al sistema.");
                return View(vm);
            }

            var result = await _signInManager.PasswordSignInAsync(
                vm.UserName, vm.Password,
                isPersistent: false,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(vm);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Administrador"))
                return RedirectToAction("Index", "DashboardAdmin");
            if (roles.Contains("Cajero"))
                return RedirectToAction("Index", "Cashier");
            if (roles.Contains("Cliente"))
                return RedirectToAction("Index", "DashboardCliente");

            await _signInManager.SignOutAsync();
            ModelState.AddModelError(string.Empty, "Tu usuario no tiene un rol asignado. Contacta al administrador.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToRoleHome()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Index", "DashboardAdmin");
            if (User.IsInRole("Cajero"))
                return RedirectToAction("Index", "Cashier");
            if (User.IsInRole("Cliente"))
                return RedirectToAction("Index", "DashboardCliente");

            return RedirectToAction("Login");
        }
    }
}
