using AutoMapper;
using PBMS.Application.Blacklist.DTOs;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Domain.Entities;

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
            .ForMember(dest => dest.OccupiedLicensePlate, opt => opt.MapFrom(src =>
                src.ParkingSessions.FirstOrDefault(ps => ps.SessionStatus == "ACTIVE" || ps.SessionStatus == "Active") != null
                    ? src.ParkingSessions.FirstOrDefault(ps => ps.SessionStatus == "ACTIVE" || ps.SessionStatus == "Active")!.LicensePlateIn
                    : src.MonthlySubscriptions.FirstOrDefault(ms => ms.MonthlySubscriptionStatus == Domain.Enums.MonthlySubscriptionStatus.Active) != null
                        ? src.MonthlySubscriptions.FirstOrDefault(ms => ms.MonthlySubscriptionStatus == Domain.Enums.MonthlySubscriptionStatus.Active)!.Vehicle!.LicensePlate
                        : null));
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
    }
}
