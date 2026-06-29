using AutoMapper;
using PBMS.Application.AuditLog.DTOs;
using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.Common;

/// <summary>
/// AutoMapper profile để ánh xạ giữa DTO và entity domain.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Zone mappings
        CreateMap<Zone, ZoneDto>();
        CreateMap<ZoneCreateRequest, Zone>();
        CreateMap<ZoneUpdateRequest, Zone>();

        // Floor mappings
        CreateMap<Floor, FloorDto>();
        CreateMap<FloorCreateRequest, Floor>();
        CreateMap<FloorUpdateRequest, Floor>();

        // ParkingSlot mappings
        CreateMap<ParkingSlot, ParkingSlotDto>()
            .ForMember(dest => dest.OccupiedLicensePlate, opt => opt.MapFrom(src => GetOccupiedLicensePlate(src)))
            .ForMember(dest => dest.Subscription, opt => opt.MapFrom(src => GetActiveSubscription(src)));
        CreateMap<ParkingSlotCreateRequest, ParkingSlot>();
        CreateMap<ParkingSlotUpdateRequest, ParkingSlot>();

        // Building mappings
        CreateMap<Building, BuildingDto>();
        CreateMap<BuildingCreateRequest, Building>();
        CreateMap<BuildingUpdateRequest, Building>();

        CreateMap<PBMS.Domain.Entities.Blacklist, BlacklistDto>()
            .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null))
            .ForMember(dest => dest.CardCode, opt => opt.MapFrom(src => src.Card != null ? src.Card.CardCode : null));

        // Incident mappings
        CreateMap<PBMS.Domain.Entities.Incident, IncidentDto>()
            .ForMember(dest => dest.IncidentName, opt => opt.MapFrom(src => src.IncidentType.IncidentName))
            .ForMember(dest => dest.LicensePlate, opt => opt.MapFrom(src => src.Session.LicensePlateIn));
        CreateMap<IncidentType, IncidentTypeDto>();

        // Pricing Config mappings
        CreateMap<SubscriptionPriceConfig, PBMS.Application.Pricing.DTOs.SubscriptionPriceConfigDto>()
            .ForMember(dest => dest.VehicleTypeName, opt => opt.MapFrom(src => src.VehicleType.TypeName));
        
        CreateMap<PenaltyConfig, PenaltyConfigDto>()
            .ForMember(dest => dest.IncidentTypeName, opt => opt.MapFrom(src => src.IncidentType.IncidentName));

        // AuditLog mappings
        CreateMap<PBMS.Domain.Entities.AuditLog, AuditLogDto>()
            .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.FullName : null));
    }

    private static string? GetOccupiedLicensePlate(ParkingSlot src)
    {
        var activeSession = src.ParkingSessions.FirstOrDefault(ps => ps.SessionStatus == "ACTIVE" || ps.SessionStatus == "Active");
        if (activeSession != null)
            return activeSession.LicensePlateIn;

        var activeSubscription = src.MonthlySubscriptions.FirstOrDefault(ms => ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active);
        if (activeSubscription != null && activeSubscription.Vehicle != null)
            return activeSubscription.Vehicle.LicensePlate;

        return null;
    }

    private static SlotSubscriptionInfoDto? GetActiveSubscription(ParkingSlot src)
    {
        var activeSubscription = src.MonthlySubscriptions.FirstOrDefault(ms => ms.MonthlySubscriptionStatus == MonthlySubscriptionStatus.Active);
        if (activeSubscription == null)
            return null;

        return new SlotSubscriptionInfoDto
        {
            SubscriptionId = activeSubscription.Id,
            AccountId = activeSubscription.AccountId,
            AccountName = activeSubscription.Account?.FullName,
            VehicleId = activeSubscription.VehicleId,
            LicensePlate = activeSubscription.Vehicle?.LicensePlate,
            Status = activeSubscription.MonthlySubscriptionStatus,
            MonthlyPrice = activeSubscription.MonthlyPrice,
            ActivatedAt = activeSubscription.ActivatedAt,
            ExpiredAt = activeSubscription.ExpiredAt
        };
    }
}
