using Microsoft.Extensions.DependencyInjection;

namespace Da3wa.Domain.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            // Register domain-level services (validators, domain events, etc.)

            return services;
        }
    }
}
