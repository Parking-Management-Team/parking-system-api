using Microsoft.Extensions.DependencyInjection;

namespace PBMS.Application;

/// <summary>
/// Extension methods for registering application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application layer services into the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: Register application services, handlers, validators, mappers, etc.
        // Example:
        // services.AddScoped<IBookingService, BookingService>();
        return services;
    }
}
