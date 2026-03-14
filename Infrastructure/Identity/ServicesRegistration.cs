using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Identity.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity
{
    public static class ServicesRegistration
    {
        public static string ToMayusculas(this string input)
        {
            return input.ToUpper();
        }
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<AppUser, IdentityRole>(options => // Configuracion del motor de Identity (Usuario y Roles)
            {
                // Configuracion de la reglas de seguridad bancaria para la contraseña
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true; // Exige simbolos
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

                options.User.RequireUniqueEmail = true; // Evita cuentas duplicadas
            })
                .AddEntityFrameworkStores<ApplicationDbContext>() // Aqui hacemos que use el DbContext unificado
                .AddDefaultTokenProviders(); // Permite generar tokens nativos

            // 2. Configuracion de la Autenticacion por JWT 
            // Nota: NO cambiamos los esquemas por defecto (se quedan en cookies: Identity.Application).
            // Registramos JWT como un esquema ADICIONAL, nombrado "Jwt", para usarlo en endpoints de API.
            services
                .AddAuthentication() // no tocar Default*Scheme aquí para no romper la redirección al login
                .AddJwtBearer("Jwt", options =>
                {
                    options.RequireHttpsMetadata = false; // En produccion esto debe ser true
                    options.SaveToken = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // Cosas que deben validar estrictamente
                        ValidateIssuerSigningKey = true, // ¿La firma criptográfica coincide con nuestra clave secreta?
                        ValidateIssuer = true, // ¿Fuimos nosotros (el banco) quienes emitimos este token?
                        ValidateAudience = true, // ¿Este token es para esta aplicación específica?
                        ValidateLifetime = true, // ¿El token sigue vivo o ya expiró?
                        ClockSkew = TimeSpan.Zero, // Por defecto, .NET le da a los tokens 5 minutos extra de vida después de que expiran (por si el reloj del servidor está desincronizado). En un sistema bancario, si la sesión dura 60 minutos, al minuto 60 con 1 segundo se debe cerrar. TimeSpan.Zero elimina ese tiempo de gracia.

                        // Datos del appsettings.json
                        ValidIssuer = configuration["JWTSettings:Issuer"],
                        ValidAudience = configuration["JWTSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["JWTSettings:Key"]!))
                    };
                });

            // (Opcional) Política para APIs que fuerce el uso de JWT sin tener que especificar el esquema en cada controlador.
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiPolicy", policy =>
                {
                    policy.AddAuthenticationSchemes("Jwt");
                    policy.RequireAuthenticatedUser();
                });
            });
        }
    }
}