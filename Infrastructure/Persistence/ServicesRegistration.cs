using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence
{
    public static class ServicesRegistration 
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration configuration) // (IOC) Inversion of Control
        {
            if( configuration.GetValue<bool>("UseInMemoryDatabase")) /* busca en tu archivo de configuración (normalmente appsettings.json) una variable booleana llamada "UseInMemoryDatabase" */
            {
                services.AddDbContext<ApplicationDbContext>( options 
                => options.UseInMemoryDatabase("Banking") );
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(
                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                        m => m.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
        }
    }
}