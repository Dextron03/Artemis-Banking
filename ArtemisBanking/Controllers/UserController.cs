using Application.Services;
using Application.ViewModels;
using Domain.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Controllers
{
    public class UserController : Controller
    {
        private readonly UserService _userService;
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserService userService, UserManager<AppUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(string role, int page = 1)
        {
            int pageSize = 20;

            var users = _userManager.Users.ToList();

            var list = new List<UserViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                if (roles.Contains("Comercio")) continue;

                var userRole = roles.FirstOrDefault();

                if (!string.IsNullOrEmpty(role) && userRole != role)
                    continue;

                list.Add(new UserViewModel
                {
                    Id = u.Id,
                    Username = u.UserName,
                    FullName = u.FirtsName + " " + u.LastName,
                    Email = u.Email,
                    Cedula = u.IdentityNumber,
                    Role = userRole,
                    IsActive = u.LockoutEnd == null
                });
            }

            list = list.OrderByDescending(u => u.Id).ToList();

            var paginated = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;

            return View(paginated);
        }
        public IActionResult Create()
        {
            var vm = new SaveUserViewModel
            {
                Roles = GetRoles()
            };

            return View("SaveUser", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaveUserViewModel vm)
        {
            if (vm.Password != vm.ConfirmPassword)
                ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");

            var existUser = await _userManager.FindByNameAsync(vm.Username);
            if (existUser != null)
                ModelState.AddModelError("Username", "El usuario ya existe");

            var existEmail = await _userManager.FindByEmailAsync(vm.Email);
            if (existEmail != null)
                ModelState.AddModelError("Email", "El correo ya existe");

            if (!ModelState.IsValid)
            {
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }

            await _userService.CreateUser(
                vm.FirstName,
                vm.LastName,
                vm.Email,
                vm.Username,
                vm.Password,
                vm.Role,
                vm.InitialAmount ?? 0
            );

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return RedirectToAction("Index");

            var vm = new SaveUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirtsName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = GetRoles()
            };

            return View("SaveUser", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Roles = GetRoles();
                return View("SaveUser", vm);
            }

            await _userService.UpdateUser(
                vm.Id,
                vm.FirstName,
                vm.LastName,
                vm.Email,
                vm.Username,
                vm.Cedula,
                vm.Password,
                vm.AdditionalAmount
            );

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Toggle(string id)
        {
            await _userService.ToggleUserStatus(id);
            return RedirectToAction("Index");
        }
        private List<SelectListItem> GetRoles()
        {
            return Enum.GetValues(typeof(Role))
                .Cast<Role>()
                .Select(r => new SelectListItem
                {
                    Value = r.ToString(),
                    Text = r.ToString()
                }).ToList();
        }
    }
}