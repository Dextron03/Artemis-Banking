using Application.DTOs.User;
using Application.Services;
using Application.ViewModels.User.Management;
using Application.ViewModels.User.Queries;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Security.Claims;

namespace ArtemisBanking.Controllers
{
    [Authorize(Roles = "Administrador")] 
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? role, string? search, int page = 1)
        {
            int pageSize = 20;
            var users = _userManager.Users.ToList();
            var list = new List<UserViewModel>();

            foreach (var u in users)
            {
                // Búsqueda por término (Nombre, Apellido, Cédula)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    string term = search.Trim().ToLower();
                    bool match = u.FirtsName.ToLower().Contains(term) ||
                                 u.LastName.ToLower().Contains(term) ||
                                 (!string.IsNullOrEmpty(u.IdentityNumber) && u.IdentityNumber.Contains(term)) ||
                                 (u.UserName != null && u.UserName.ToLower().Contains(term));
                    if (!match) continue;
                }

                var roles = await _userManager.GetRolesAsync(u);
                if (roles.Contains("Comercio")) continue;
                var userRole = roles.FirstOrDefault();
                
                // Filtrado por Rol
                if (!string.IsNullOrEmpty(role) && userRole != role) continue;

                list.Add(new UserViewModel
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty,
                    FullName = $"{u.FirtsName} {u.LastName}",
                    Email = u.Email ?? string.Empty,
                    Cedula = string.IsNullOrEmpty(u.IdentityNumber) ? "N/A" : u.IdentityNumber,
                    Role = userRole ?? string.Empty,
                    IsActive = u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.Now
                });
            }

            list = list.OrderByDescending(u => u.Id).ToList();

            int totalCount = list.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var paginated = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vm = new UserIndexViewModel
            {
                Users = paginated,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize,
                SelectedRole = role,
                SearchTerm = search
            };

            return View(vm);
        }

        public IActionResult Create()
        {
            var vm = new SaveUserViewModel { Roles = GetRoles() };
            return View("SaveUser", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }

            var tempUser = new AppUser
            {
                FirtsName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                UserName = vm.Username,
                IdentityNumber = vm.Cedula,
                IsActive = false,
                LockoutEnd = DateTimeOffset.MaxValue
            };

            try
            {
                string userId = await _userService.CreateUserAndGetId(
                    vm.FirstName, vm.LastName, vm.Email, vm.Username,
                    vm.Cedula, vm.Password!, vm.Role, vm.InitialAmount ?? 0);

                var user = await _userManager.FindByIdAsync(userId);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
                var encodedToken = System.Net.WebUtility.UrlEncode(token);
                var link = Url.Action("ConfirmEmail", "User",
                    new { userId = user!.Id, token = encodedToken },
                    Request.Scheme);

                await _userService.SendActivationEmail(user.Email!, user.FirtsName, link!);

                TempData["Success"] = "Usuario creado. Se envió correo de activación.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["Error"] = "No puedes editar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction("Index");

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();

            var vm = new SaveUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirtsName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Username = user.UserName ?? string.Empty,
                Cedula = user.IdentityNumber,
                Role = userRole ?? string.Empty,
                Roles = GetRoles()
            };

            return View("SaveUser", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (vm.Id == currentUserId)
            {
                TempData["Error"] = "No puedes editar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!string.IsNullOrEmpty(vm.Password))
            {
                if (string.IsNullOrEmpty(vm.ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Debe confirmar la contraseña");
                else if (vm.Password != vm.ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
            }

            if (!ModelState.IsValid)
            {
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }

            try
            {
                await _userService.UpdateUser(
                    vm.Id!, vm.FirstName, vm.LastName, vm.Email,
                    vm.Username, vm.Cedula, vm.Password ?? string.Empty,
                    vm.AdditionalAmount);

                TempData["Success"] = "Usuario actualizado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["Error"] = "No puedes modificar el estado de tu propia cuenta.";
                return RedirectToAction("Index");
            }

            await _userService.ToggleUserStatus(id);
            TempData["Success"] = "Estado del usuario actualizado.";
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var decodedToken = WebUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
                return BadRequest("Error al confirmar el correo. El enlace puede haber expirado.");

            user.LockoutEnd = null;
            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return View(); 
        }

        private List<SelectListItem> GetRoles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem("Administrador", "Administrador"),
                new SelectListItem("Cajero", "Cajero"),
                new SelectListItem("Cliente", "Cliente")
            };
        }
    }
}