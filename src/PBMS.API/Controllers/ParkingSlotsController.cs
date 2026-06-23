using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Common;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;
using PBMS.Domain.Enums;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý vị trí đỗ xe (Parking Slot).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParkingSlotsController : ControllerBase
{
    private readonly IParkingSlotService _slotService;

    public ParkingSlotsController(IParkingSlotService slotService)
    {
        _slotService = slotService;
    }

    /// <summary>
    /// Tạo vị trí đỗ xe mới.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSlot([FromBody] ParkingSlotCreateRequest request)
    {
        var slot = await _slotService.CreateSlotAsync(request);
        var response = BaseResponse<ParkingSlotDto>.Ok(slot, "Parking slot created successfully.");
        return CreatedAtAction(nameof(GetSlotById), new { id = slot.Id }, response);
    }

    /// <summary>
    /// Lấy thông tin vị trí đỗ xe theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSlotById(int id)
    {
        var slot = await _slotService.GetSlotByIdAsync(id);
        return Ok(BaseResponse<ParkingSlotDto>.Ok(slot));
    }

    /// <summary>
    /// Lấy danh sách tất cả các vị trí đỗ xe.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllSlots()
    {
        var slots = await _slotService.GetAllSlotsAsync();
        return Ok(BaseResponse<IEnumerable<ParkingSlotDto>>.Ok(slots));
    }

    /// <summary>
    /// Lấy danh sách vị trí đỗ xe theo khu vực (Zone), hỗ trợ lọc nâng cao theo nhiều tiêu chí.
    /// </summary>
    [HttpGet("zone/{zoneId}")]
    public async Task<IActionResult> GetSlotsByZone(
        int zoneId, 
        [FromQuery] List<SlotStatus>? statuses = null,
        [FromQuery] List<int>? vehicleTypeIds = null,
        [FromQuery] string? search = null)
    {
        var slots = await _slotService.GetSlotsByZoneAsync(zoneId, statuses, vehicleTypeIds, search);
        return Ok(BaseResponse<IEnumerable<ParkingSlotDto>>.Ok(slots));
    }

    /// <summary>
    /// Lấy danh sách vị trí đỗ xe theo phân trang.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetSlotsPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _slotService.GetSlotsPagedAsync(pageIndex, pageSize);
        return Ok(BaseResponse<PagedResult<ParkingSlotDto>>.Ok(result));
    }

    /// <summary>
    /// Cập nhật thông tin vị trí đỗ xe.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSlot(int id, [FromBody] ParkingSlotUpdateRequest request)
    {
        var slot = await _slotService.UpdateSlotAsync(id, request);
        return Ok(BaseResponse<ParkingSlotDto>.Ok(slot, "Parking slot updated successfully."));
    }

    /// <summary>
    /// Xóa vị trí đỗ xe.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        await _slotService.DeleteSlotAsync(id);
        return Ok(BaseResponse<string>.Ok(id.ToString(), "Parking slot deleted successfully."));
    }

    /// <summary>
    /// Khóa vị trí đỗ xe (Block).
    /// </summary>
    [HttpPost("{id}/block")]
    public async Task<IActionResult> BlockSlot(int id, [FromBody] SlotStatusChangeRequest request)
    {
        var slot = await _slotService.BlockSlotAsync(id, request);
        return Ok(BaseResponse<ParkingSlotDto>.Ok(slot, "Parking slot blocked successfully."));
    }

    /// <summary>
    /// Mở khóa vị trí đỗ xe (Unblock).
    /// </summary>
    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> UnblockSlot(int id, [FromBody] SlotStatusChangeRequest request)
    {
        var slot = await _slotService.UnblockSlotAsync(id, request);
        return Ok(BaseResponse<ParkingSlotDto>.Ok(slot, "Parking slot unblocked successfully."));
    }

    /// <summary>
    /// Đặt vị trí đỗ xe vào trạng thái bảo trì.
    /// </summary>
    [HttpPost("{id}/maintenance")]
    public async Task<IActionResult> SetMaintenanceSlot(int id, [FromBody] SlotStatusChangeRequest request)
    {
        var slot = await _slotService.SetMaintenanceSlotAsync(id, request);
        return Ok(BaseResponse<ParkingSlotDto>.Ok(slot, "Parking slot set to maintenance successfully."));
    }
}
