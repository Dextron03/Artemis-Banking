using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using AutoMapper;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var dto = _mapper.Map<LoginDto>(vm);

            var result = await _authService.LoginAsync(dto);

            if (!result)
            {
                ModelState.AddModelError("", "Credenciales incorrectas");
                return View(vm);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
