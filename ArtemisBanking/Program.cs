using Application;
using Application.Jobs;
using Infrastructure.Identity;
using Infrastructure.Identity.Seeds;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Shared;
using Hangfire;

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
builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddSharedLeyer(builder.Configuration);

builder.Services.AddHangfire(config => config.UseSqlServerStorage( builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<LoanOverdueJob>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    // Opcional el restringir acceso al dashboard
});

RecurringJob.AddOrUpdate<LoanOverdueJob>(
    "mark-overdue-shares",
    job => job.MarkOverdueSharesAsync(),
    Cron.Daily(0, 0),   
    new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

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