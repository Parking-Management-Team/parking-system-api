using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý tòa nhà (Building).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    /// <summary>
    /// Tạo tòa nhà mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBuilding([FromBody] BuildingCreateRequest request)
    {
        var building = await _buildingService.CreateBuildingAsync(request);
        var response = BaseResponse<BuildingDto>.Ok(building, "Building created successfully.");
        return CreatedAtAction(nameof(GetBuildingById), new { id = building.Id }, response);
    }

    /// <summary>
    /// Lấy thông tin tòa nhà theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuildingById(int id)
    {
        var building = await _buildingService.GetBuildingByIdAsync(id);
        return Ok(BaseResponse<BuildingDto>.Ok(building));
    }

    /// <summary>
    /// Lấy danh sách tất cả các tòa nhà.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllBuildings()
    {
        var buildings = await _buildingService.GetAllBuildingsAsync();
        return Ok(BaseResponse<IEnumerable<BuildingDto>>.Ok(buildings));
    }

    /// <summary>
    /// Lấy danh sách tòa nhà theo phân trang.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetBuildingsPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _buildingService.GetBuildingsPagedAsync(pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<BuildingDto>>.Ok(result));
    }

    /// <summary>
    /// Cập nhật thông tin tòa nhà.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBuilding(int id, [FromBody] BuildingUpdateRequest request)
    {
        var building = await _buildingService.UpdateBuildingAsync(id, request);
        return Ok(BaseResponse<BuildingDto>.Ok(building, "Building updated successfully."));
    }

    /// <summary>
    /// Xóa tòa nhà.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBuilding(int id)
    {
        await _buildingService.DeleteBuildingAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Building deleted successfully."));
    }
}
