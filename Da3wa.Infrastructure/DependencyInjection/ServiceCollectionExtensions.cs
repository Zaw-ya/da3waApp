namespace Da3wa.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Example: register DbContext if a connection string is present
            // var conn = configuration.GetConnectionString("DefaultConnection");
            // if (!string.IsNullOrWhiteSpace(conn))
            // {
            //     services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(conn));
            // }

            // Register infrastructure services

            return services;
        }
    }
}
