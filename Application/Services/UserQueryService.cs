using Application.Interfaces;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserQueryService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserQueryService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<AppUser?> GetByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<List<AppUser>> GetActiveClientsAsync()
        {
            return await _userManager.Users
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsUserActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null && user.IsActive;
        }
    }
}
