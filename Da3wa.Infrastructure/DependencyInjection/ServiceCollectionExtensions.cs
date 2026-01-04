using Da3wa.Application.Interfaces.Repositories;
using Da3wa.Infrastructure.Persistence;
using Da3wa.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Da3wa.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(conn))
            {
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(conn));
            }

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register infrastructure services

            return services;
        }
    }
}
