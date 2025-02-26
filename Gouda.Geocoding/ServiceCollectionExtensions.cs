using Microsoft.Extensions.DependencyInjection;

namespace Gouda.Geocoding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGeocoding(this IServiceCollection services)
    {
        return services.AddScoped<GeocodingService>();
    }
}
