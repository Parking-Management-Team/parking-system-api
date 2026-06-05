using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;

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
        // Register application services
        services.AddScoped<IVehicleTypeService, VehicleTypeService>();

        return services;
    }
}
