using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class ServiceRegistration
    {
        // El "this IServiceCollection" es lo que lo convierte en un Extension Method
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<UserService>();
            services.AddScoped<LoanService>();
            services.AddTransient<ICashierService, CashierService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        }
    
    }
}