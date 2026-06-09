using AutoMapper;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;

namespace PBMS.Application.ParkingStructure.Services;

/// <summary>
/// Triển khai service cho logic nghiệp vụ Building.
/// </summary>
public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public BuildingService(IBuildingRepository buildingRepository, IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<BuildingDto> CreateBuildingAsync(BuildingCreateRequest request)
    {
        // 1. Kiểm tra Building Code duy nhất
        var codeExists = await _buildingRepository.BuildingCodeExistsAsync(request.Code);
        if (codeExists)
        {
            throw new ValidationException($"Building code '{request.Code}' already exists.");
        }

        // 2. Tạo entity
        var building = new Building
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name,
            Address = request.Address,
            TotalFloor = request.TotalFloor,
            Status = BuildingStatus.Available
        };

        await _buildingRepository.AddAsync(building);
        return _mapper.Map<BuildingDto>(building);
    }

    public async Task<BuildingDto> GetBuildingByIdAsync(int id)
    {
        var building = await _buildingRepository.GetByIdAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        return _mapper.Map<BuildingDto>(building);
    }

    public async Task<IEnumerable<BuildingDto>> GetAllBuildingsAsync()
    {
        var buildings = await _buildingRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<BuildingDto>>(buildings);
    }

    public async Task<PagedResult<BuildingDto>> GetBuildingsPagedAsync(int pageIndex, int pageSize)
    {
        var buildings = await _buildingRepository.GetAllAsync();
        var totalCount = buildings.Count();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedBuildings = buildings
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var buildingDtos = _mapper.Map<IEnumerable<BuildingDto>>(pagedBuildings);

        return new PagedResult<BuildingDto>
        {
            Items = buildingDtos,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageIndex = pageIndex,
            PageSize = pageSize
        };
    }

    public async Task<BuildingDto> UpdateBuildingAsync(int id, BuildingUpdateRequest request)
    {
        var building = await _buildingRepository.GetByIdAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        // Kiểm tra Building Code mới nếu thay đổi
        var newCode = request.Code.Trim().ToUpper();
        if (building.Code != newCode)
        {
            var codeExists = await _buildingRepository.BuildingCodeExistsAsync(newCode);
            if (codeExists)
            {
                throw new ValidationException($"Building code '{newCode}' already exists.");
            }
        }

        building.Code = newCode;
        building.Name = request.Name;
        building.Address = request.Address;
        building.TotalFloor = request.TotalFloor;
        building.Status = request.Status;

        _buildingRepository.Update(building);
        return _mapper.Map<BuildingDto>(building);
    }

    public async Task DeleteBuildingAsync(int id)
    {
        var building = await _buildingRepository.GetBuildingWithDetailsAsync(id);
        if (building == null)
        {
            throw new NotFoundException("Building", id);
        }

        // Logic bảo vệ: Không cho xóa nếu tòa nhà vẫn còn tầng (Floor)
        if (building.Floors.Any())
        {
            throw new ValidationException($"Cannot delete building '{building.Code}' because it contains floors.");
        }

        await _buildingRepository.RemoveAsync(building);
    }
}
