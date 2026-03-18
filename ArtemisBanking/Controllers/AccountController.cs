using Application.DTOs.Login;
using Application.Interfaces;
using Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AccountController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var dto = _mapper.Map<LoginDto>(vm);
            var result = await _authService.LoginAsync(dto);

            if (result.HasError)
            {
                ModelState.AddModelError("", result.Error);
                return View(vm);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (result.Roles.Contains("Administrador"))
                return RedirectToAction("Index", "DashboardAdmin");
            if (result.Roles.Contains("Cliente"))
                return RedirectToAction("Index", "DashboardCliente");
            if (result.Roles.Contains("Cajero"))
                return RedirectToAction("Index", "DashboardCajero");

            return RedirectToAction("Welcome", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}