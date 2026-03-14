using Application.Interfaces;
using Application.Services;
using Infrastructure.Identity.Seeds;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
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

// tus servicios
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();