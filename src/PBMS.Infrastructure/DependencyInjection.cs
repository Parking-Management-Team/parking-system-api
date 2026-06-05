using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Contracts;
using PBMS.Infrastructure.Data;
using PBMS.Infrastructure.Repositories;

namespace PBMS.Infrastructure;

/// <summary>
/// Phương thức mở rộng để đăng ký dịch vụ tầng Infrastructure.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các dịch vụ tầng Infrastructure vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ đã được cập nhật.</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Đăng ký DbContext, repository, dịch vụ ngoài và cấu hình infrastructure.
        // Ví dụ:
        // services.AddDbContext<ApplicationDbContext>(options =>
        //     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Register ParkingStructure repositories
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IZoneRepository, ZoneRepository>();

        return services;
    }
}
