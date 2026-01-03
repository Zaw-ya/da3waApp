namespace Da3wa.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register application services

            //services.AddSingleton(configuration);
            return services;
        }
    }
}
