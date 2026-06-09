using AutoMapper;
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
        CreateMap<ParkingSlot, ParkingSlotDto>();
        CreateMap<ParkingSlotCreateRequest, ParkingSlot>();
        CreateMap<ParkingSlotUpdateRequest, ParkingSlot>();

        // Building mappings
        CreateMap<Building, BuildingDto>();
        CreateMap<BuildingCreateRequest, Building>();
        CreateMap<BuildingUpdateRequest, Building>();
    }
}
