using Da3wa.Application.DependencyInjection;
using Da3wa.Domain.DependencyInjection;
using Da3wa.Domain.Entities;
using Da3wa.Infrastructure.DependencyInjection;
using Da3wa.Infrastructure.Persistence;
using Da3wa.WebUI.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Da3wa.WebUI.Services;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.WebUI.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebUI(this IServiceCollection services, IConfiguration configuration)
        {
            // Register exception handling filter globally
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<ExceptionHandlingFilter>();
                options.Filters.Add<ControllerActionFilter>();
                
             
            });
            // ==============================================
            // Register DbContext
            // ==============================================
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ==============================================
            // Register ASP.NET Core Identity
            // ==============================================
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddTransient<IEmailSender, EmailSender>();

            // Registers controllers, application and infrastructure services
            // Note: AddControllersWithViews is already called above with filters
            services.AddRazorPages();

            services.AddDomain();
            services.AddApplication(configuration);
            services.AddInfrastructure(configuration);

            return services;
        }
    }
}
