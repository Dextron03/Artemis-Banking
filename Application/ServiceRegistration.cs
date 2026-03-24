using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application
{
    public static class ServiceRegistration
    {
        // El "this IServiceCollection" es lo que lo convierte en un Extension Method
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<UserService>();
            services.AddScoped<ILoanService,LoanService>();
            services.AddTransient<ICashierService, CashierService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ISavingsAccountService, SavingsAccountService>();
            services.AddScoped<IUserService, UserQueryService>();

        }
    
    }
}