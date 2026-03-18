using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.AuthResponse;
using Application.DTOs.Login;
using Application.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthService(SignInManager<AppUser> signInManager,
                           UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var response = new AuthResponseDto();

            var user = await _userManager.FindByNameAsync(dto.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Error = "Usuario no encontrado";
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = "Credenciales incorrectas";
                return response;
            }

            var roles = await _userManager.GetRolesAsync(user);

            response.UserId = user.Id;
            response.Roles = roles;

            return response;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync(); 
        }
    }
}
