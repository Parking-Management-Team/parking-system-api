using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PBMS.Infrastructure.Data;
using PBMS.Infrastructure.Repositories;
using PBMS.Application.Vehicle.Interfaces;

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
        if (bool.TryParse(configuration["VehicleType:UseMockData"], out var useMockData) && useMockData)
        {
            services.AddSingleton<IVehicleTypeRepository, MockVehicleTypeRepository>();
            return services;
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        
        // In development, try SQL Server first (LocalDB), then fall back to PostgreSQL
        if (env == "Development")
        {
            try
            {
                // Try to use SQL Server (LocalDB) for development - easier to set up
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(connectionString));

                // Try to create/migrate the database
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                }
            }
            catch (Exception sqlEx)
            {
                Console.WriteLine($"Warning: Could not connect to SQL Server: {sqlEx.Message}");
                Console.WriteLine("Falling back to in-memory database for testing...");
                
                // Remove the failed DbContext and use in-memory database
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("pbms_development_db"));
                
                // Initialize in-memory database
                var serviceProvider = services.BuildServiceProvider();
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                }
            }
        }
        else
        {
            // Production: Use PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Ensure database is created with all tables
            var serviceProvider = services.BuildServiceProvider();
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.EnsureCreated();
            }
        }

        // Register repositories
        services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();

        return services;
    }
}
