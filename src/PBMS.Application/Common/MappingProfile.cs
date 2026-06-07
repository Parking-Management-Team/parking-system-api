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
    }
}
