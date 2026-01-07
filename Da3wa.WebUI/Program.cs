using Da3wa.Infrastructure.Data;
using Da3wa.WebUI.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;


// Global usings provide DI extension visibility.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddWebUI(builder.Configuration);

// Register seeding service
builder.Services.AddScoped<ApplicationDbContextSeed>();



// Add the processing server as IHostedService
builder.Services.AddHangfireServer();

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seeder = services.GetRequiredService<ApplicationDbContextSeed>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles");
    }
}

// Configure the HTTP request pipeline.
// Custom error handler for UI (re-executes at /Home/Error) - MUST be first to catch rethrown exceptions
app.UseExceptionHandler("/Home/Error");

// Global exception handling for APIs (returns JSON, rethrows for UI)
app.UseMiddleware<Da3wa.WebUI.Middleware.GlobalExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (accessible at /hangfire)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


app.Run();
