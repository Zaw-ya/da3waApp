using Da3wa.Application.Interfaces;
using Da3wa.Application.Services;
using Da3wa.Application.Mappings;

namespace Da3wa.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register AutoMapper
            services.AddAutoMapper(typeof(UserMappingProfile));

            // Register application services
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IGuestService, GuestService>();
            services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();


            //services.AddSingleton(configuration);
            return services;
        }
    }
}
