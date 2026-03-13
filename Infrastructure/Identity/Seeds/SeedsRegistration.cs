using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity.Seeds
{
    public static class SeedsRegistration
    {
        public static async Task RunIdentitySeedsAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    // 1. Pedimos prestados los "Managers" de Identity al contenedor
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // 2. Ejecutamos tus clases de Seeding
                    await DefaultRoles.SeedAsync(roleManager);
                    await DefaultAdminUser.SeedAsync(userManager);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger<InvalidProgramException>();
                    logger.LogError(ex, "Ocurrió un error al intentar sembrar los datos en la base de datos.");
                }
            }
        }
    }
}