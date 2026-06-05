using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Application.ParkingStructure.Services;

namespace PBMS.Application;
/// <summary>
/// Phương thức mở rộng để đăng ký dịch vụ tầng Application.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký các dịch vụ tầng Application vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ.</param>
    /// <returns>Tập hợp dịch vụ đã được cập nhật.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // TODO: Đăng ký các dịch vụ ứng dụng, handler, validator, mapper, v.v.
        // Ví dụ:
        // services.AddScoped<IBookingService, BookingService>();
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection));
        services.AddScoped<IZoneService, ZoneService>();
        return services;
    }
}
