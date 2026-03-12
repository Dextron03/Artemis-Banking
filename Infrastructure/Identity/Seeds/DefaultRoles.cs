using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            // Verificamos si los roles ya existen en la base de datos para no duplicalos 
            if(!await roleManager.RoleExistsAsync(Role.Administrador.ToString())) 
                await roleManager.CreateAsync(new IdentityRole(Role.Administrador.ToString()));

            if(!await roleManager.RoleExistsAsync(Role.Cajero.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Role.Cajero.ToString()));

            if(!await roleManager.RoleExistsAsync(Role.Cliente.ToString()))
                await roleManager.CreateAsync(new IdentityRole(Role.Cliente.ToString()));
        }
    }
}