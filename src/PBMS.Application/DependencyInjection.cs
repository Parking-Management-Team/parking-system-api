using Microsoft.Extensions.DependencyInjection;
using PBMS.Application.Vehicle.Interfaces;
using PBMS.Application.Vehicle.Services;
using PBMS.Application.Auth.Interfaces;
using PBMS.Application.Auth.Services;
using PBMS.Application.Card.Interfaces;
using PBMS.Application.Card.Services;
using PBMS.Application.ParkingSession.Interfaces;
using PBMS.Application.ParkingSession.Services;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Application.ParkingStructure.Services;
using PBMS.Application.Pricing.Interfaces;
using PBMS.Application.Pricing.Services;
using PBMS.Application.Accounts;
using PBMS.Application.Blacklist.Interfaces;
using PBMS.Application.Blacklist.Services;
using PBMS.Application.Incident.Interfaces;
using PBMS.Application.Incident.Services;
using PBMS.Application.Payment.Interfaces;
using PBMS.Application.Payment.Services;
using PBMS.Application.Revenue.Interfaces;
using PBMS.Application.Revenue.Services;
using PBMS.Application.MonthlyCard.Interfaces;
using PBMS.Application.MonthlyCard.Services;
using PBMS.Application.Booking.Interfaces;
using PBMS.Application.Booking.Services;
using PBMS.Application.AuditLog.Interfaces;
using PBMS.Application.AuditLog.Services;



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
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, bool useInMemoryParkingSession = false)
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
        services.AddScoped<IParkingSessionService, ParkingSessionService>();
        services.AddScoped<IBuildingService, BuildingService>();
        services.AddScoped<IBlacklistService, BlacklistService>();
        services.AddScoped<IIncidentService, IncidentService>();
        services.AddScoped<IIncidentTypeService, IncidentTypeService>();

        // Pricing module
        services.AddScoped<IPricingPolicyService, PricingPolicyService>();
        services.AddScoped<IFeeCalculationService, FeeCalculationService>();
        services.AddScoped<PBMS.Application.Common.IFeeCalculatorService, PBMS.Application.Common.FeeCalculatorService>();
        services.AddScoped<PBMS.Application.Pricing.Interfaces.ISubscriptionPriceConfigService, PBMS.Application.Pricing.Services.SubscriptionPriceConfigService>();
        services.AddScoped<PBMS.Application.Incident.Interfaces.IPenaltyConfigService, PBMS.Application.Incident.Services.PenaltyConfigService>();
        services.AddScoped<IAccountService, AccountService>();
        if (useInMemoryParkingSession)
        {
            services.AddSingleton<IParkingSessionService, InMemoryParkingSessionService>();
        }
        else
        {
            services.AddScoped<IParkingSessionService, ParkingSessionService>();
        }
        // Payment module
        services.AddScoped<IPaymentService, PaymentService>();
        // Revenue module
        services.AddScoped<IRevenueService, RevenueService>();
        // Monthly Subscription module
        services.AddScoped<IMonthlySubscriptionService, MonthlySubscriptionService>();

        // Booking module
        services.AddScoped<IBookingService, BookingService>();

        // AuditLog module
        services.AddScoped<IAuditLogService, AuditLogService>();

        // Dashboard module
        services.AddScoped<PBMS.Application.Common.Interfaces.IDashboardService, PBMS.Application.Common.Services.DashboardService>();

        return services;



    }
}
