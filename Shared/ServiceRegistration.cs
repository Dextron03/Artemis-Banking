using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using Shared.Settings;

namespace Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedLeyer(this IServiceCollection services, IConfiguration  configuration)
        {
            services.AddTransient<IEmailService, EmailService>();
            services.Configure<EmailSettings>(
            configuration.GetSection("EmailSettings"));
        }
    }
}