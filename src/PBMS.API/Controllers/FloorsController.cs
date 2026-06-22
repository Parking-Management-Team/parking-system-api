using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý tầng (Floor).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FloorsController : ControllerBase
{
    private readonly IFloorService _floorService;

    public FloorsController(IFloorService floorService)
    {
        _floorService = floorService;
    }

    /// <summary>
    /// Tạo tầng mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateFloor([FromBody] FloorCreateRequest request)
    {
        var floor = await _floorService.CreateFloorAsync(request);
        var response = BaseResponse<FloorDto>.Ok(floor, "Floor created successfully.");
        return CreatedAtAction(nameof(GetFloorById), new { id = floor.Id }, response);
    }

    /// <summary>
    /// Lấy thông tin tầng theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFloorById(int id)
    {
        var floor = await _floorService.GetFloorByIdAsync(id);
        return Ok(BaseResponse<FloorDto>.Ok(floor));
    }

    /// <summary>
    /// Lấy tất cả tầng.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllFloors()
    {
        var floors = await _floorService.GetAllFloorsAsync();
        return Ok(BaseResponse<IEnumerable<FloorDto>>.Ok(floors));
    }

    /// <summary>
    /// Lấy danh sách tầng theo tòa nhà.
    /// </summary>
    [HttpGet("building/{buildingId}")]
    public async Task<IActionResult> GetFloorsByBuilding(int buildingId)
    {
        var floors = await _floorService.GetFloorsByBuildingAsync(buildingId);
        return Ok(BaseResponse<IEnumerable<FloorDto>>.Ok(floors));
    }

    /// <summary>
    /// Lấy danh sách tầng theo phân trang.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetFloorsPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _floorService.GetFloorsPagedAsync(pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<FloorDto>>.Ok(result));
    }

    /// <summary>
    /// Cập nhật thông tin tầng.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFloor(int id, [FromBody] FloorUpdateRequest request)
    {
        var floor = await _floorService.UpdateFloorAsync(id, request);
        return Ok(BaseResponse<FloorDto>.Ok(floor, "Floor updated successfully."));
    }

    /// <summary>
    /// Lấy thông tin sức chứa của tầng.
    /// </summary>
    [HttpGet("{id}/capacity")]
    public async Task<IActionResult> GetFloorCapacity(int id)
    {
        var capacity = await _floorService.GetFloorCapacityAsync(id);
        return Ok(BaseResponse<CapacityDto>.Ok(capacity));
    }

    /// <summary>
    /// Lấy thống kê số lượng slot theo tầng của building (có filter theo loại xe và status)
    /// </summary>
    [HttpGet("building/{buildingId}/slot-summary")]
    public async Task<IActionResult> GetFloorsSlotSummary(int buildingId, [FromQuery] int? vehicleTypeId = null, [FromQuery] string? status = null)
    {
        var summaries = await _floorService.GetFloorsSlotSummaryAsync(buildingId, vehicleTypeId, status);
        return Ok(BaseResponse<IEnumerable<FloorSlotSummaryDto>>.Ok(summaries));
    }

    /// <summary>
    /// Xóa tầng.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFloor(int id)
    {
        await _floorService.DeleteFloorAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Floor deleted successfully."));
    }
}
