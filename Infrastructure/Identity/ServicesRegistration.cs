using System;
using Infrastructure.Identity.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity
{
    public static class ServiceRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Configuración del motor de Identity (Esto se queda igual)
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // 2. NUEVO: Configuración de Cookies para MVC
            services.ConfigureApplicationCookie(options =>
            {
                // ¿A dónde redirigir si alguien sin login intenta entrar a una página protegida?
                options.LoginPath = "/Account/Login"; 
                
                // ¿A dónde redirigir si un Cliente intenta entrar a una vista de Cajero?
                options.AccessDeniedPath = "/Account/AccessDenied"; 
                
                // Tiempo de vida de la sesión (ej. 60 minutos)
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
                
                // Si el usuario está usando el sistema, la cookie extiende su tiempo de vida
                options.SlidingExpiration = true; 
            });
        }
    }
}