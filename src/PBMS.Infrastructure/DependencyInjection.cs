using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PBMS.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure services into the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Register DbContext, repositories, external services, and infrastructure settings.
        // Example:
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        // services.AddScoped<IBookingRepository, BookingRepository>();
        return services;
    }
}
