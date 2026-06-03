using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Auth.Services;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Card.Services;

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
        // Auth module
        services.AddScoped<IAuthService, AuthService>();

        // Card Management module
        // Scoped: mỗi HTTP request tạo một instance mới → an toàn với EF Core DbContext
        services.AddScoped<ICardService, CardService>();

        return services;
    }
}
