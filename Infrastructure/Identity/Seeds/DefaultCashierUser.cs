using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Seeds
{
    public static class DefaultCashierUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            var defaultUser = new AppUser
            {
                UserName = "Kevin",
                Email = "MrGarcia@gmail.com",
                FirtsName = "Therian69",
                LastName = "Garcia",
                IdentityNumber = "00000000000",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);

            if(user == null)
            {
                await userManager.CreateAsync(defaultUser, "456Pa$$word!");
                await userManager.AddToRoleAsync(defaultUser, Role.Cajero.ToString());
            }
        }
    }   
}