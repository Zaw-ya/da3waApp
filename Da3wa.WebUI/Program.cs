using Da3wa.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


// Global usings provide DI extension visibility.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddWebUI(builder.Configuration);

// Register seeding service
builder.Services.AddScoped<ApplicationDbContextSeed>();

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
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


app.Run();
