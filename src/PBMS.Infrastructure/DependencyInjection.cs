using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Contracts;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Infrastructure.Data;
using PBMS.Infrastructure.ExternalServices;
using PBMS.Infrastructure.Repositories;
using PBMS.Application.Payment.Interfaces;


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

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Card Management — Repository
        services.AddScoped<ICardRepository, CardRepository>();

        // Payment Management — Repository
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Blacklist Management — Repository
        services.AddScoped<IBlacklistRepository, BlacklistRepository>();

        // Incident Management — Repository
        services.AddScoped<IIncidentRepository, IncidentRepository>();

        //Google OauthServiceDI
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        
        // Cung cấp HttpContext cho CurrentUserService
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Đăng ký repository chung
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        // Đăng ký repository Zone
        services.AddScoped<IZoneRepository, ZoneRepository>();

        // Đăng ký repository Floor
        services.AddScoped<IFloorRepository, FloorRepository>();

        // Đăng ký repository ParkingSlot
        services.AddScoped<IParkingSlotRepository, ParkingSlotRepository>();
        services.AddScoped<IParkingSessionRepository, ParkingSessionRepository>();

        // Đăng ký repository Building
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IMonthlySubscriptionRepository, MonthlySubscriptionRepository>();

        // Đăng ký repository Booking
        services.AddScoped<IBookingRepository, BookingRepository>();


        // Pricing — Repository
        services.AddScoped<IPricingPolicyRepository, PricingPolicyRepository>();
        services.AddScoped<ISubscriptionPriceConfigRepository, SubscriptionPriceConfigRepository>();
        services.AddScoped<IPenaltyConfigRepository, PenaltyConfigRepository>();

        // VNPay Gateway
        services.AddScoped<IVNPayGateway, VNPayGateway>();



        return services;
    }
}
