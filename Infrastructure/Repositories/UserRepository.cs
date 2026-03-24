using Domain.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;

        public UserRepository(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsUserActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.IsActive ?? false;
        }

        public async Task SetUserActiveAsync(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado");

            user.IsActive = isActive;
            user.LockoutEnd = isActive ? null : DateTimeOffset.MaxValue;

            await _userManager.UpdateAsync(user);
        }
    }
}
