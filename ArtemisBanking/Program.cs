using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Identity;
using Infrastructure.Identity.Seeds;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Shared.Services;
using Shared.Settings;
// ...

var builder = WebApplication.CreateBuilder(args);

// 1) MVC: agrega una política global que requiera estar autenticado
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// 2) Infraestructura (Identity + EF, etc.)
builder.Services.AddIdentityInfrastructure(builder.Configuration);   // ya registra el esquema Identity.Application
builder.Services.AddPersistenceLayerIoc(builder.Configuration);

// 3) Configura el cookie EXISTENTE de Identity (no lo vuelvas a registrar)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";           // ← ruta de login
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

/*             // Registrar Identity
            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); */
// tus servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();
// seeds
    await app.RunIdentitySeedsAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();