using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PBMS.Infrastructure.Data;
using PBMS.Infrastructure.Repositories;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Infrastructure.ExternalServices;

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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITokenService, TokenService>();

        // Card Management — Repository
        services.AddScoped<ICardRepository, CardRepository>();

        //Google OauthServiceDI
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        // Đăng ký repository chung
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Đăng ký repository Zone
        services.AddScoped<IZoneRepository, ZoneRepository>();

        // Đăng ký repository Floor
        services.AddScoped<IFloorRepository, FloorRepository>();

        // Đăng ký repository ParkingSlot
        services.AddScoped<IParkingSlotRepository, ParkingSlotRepository>();

        // Đăng ký repository Building
        services.AddScoped<IBuildingRepository, BuildingRepository>();

        return services;
    }
}
