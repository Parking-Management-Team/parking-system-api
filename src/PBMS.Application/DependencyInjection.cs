using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Auth.Services;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Card.Services;
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
        // Auth module
        services.AddScoped<IAuthService, AuthService>();

        // Card Management module
        // Scoped: mỗi HTTP request tạo một instance mới → an toàn với EF Core DbContext
        services.AddScoped<ICardService, CardService>();
        services.AddScoped<IVehicleTypeService, VehicleTypeService>();
        services.AddScoped<IVehicleService, VehicleService>();

        // TODO: Đăng ký các dịch vụ ứng dụng, handler, validator, mapper, v.v.
        // Ví dụ:
        // services.AddScoped<IBookingService, BookingService>();
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection));
        services.AddScoped<IZoneService, ZoneService>();
        services.AddScoped<IFloorService, FloorService>();
        services.AddScoped<IParkingSlotService, ParkingSlotService>();
        services.AddScoped<IBuildingService, BuildingService>();
        return services;
    }
}
