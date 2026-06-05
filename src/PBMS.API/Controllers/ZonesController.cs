using Microsoft.AspNetCore.Mvc;
using PBMS.Application.ParkingStructure.DTOs;
using PBMS.Application.ParkingStructure.Interfaces;

namespace PBMS.API.Controllers;

/// <summary>
/// Controller quản lý zone.
/// Cung cấp các endpoint CRUD cho zone.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ZonesController : ControllerBase
{
    private readonly IZoneService _zoneService;

    public ZonesController(IZoneService zoneService)
    {
        _zoneService = zoneService;
    }

    /// <summary>
    /// Tạo zone mới.
    /// </summary>
    /// <param name="request">Yêu cầu tạo zone.</param>
    /// <returns>Zone vừa tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateZone([FromBody] ZoneCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _zoneService.CreateZoneAsync(request);
        
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetZoneById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Lấy thông tin zone theo ID.
    /// </summary>
    /// <param name="id">ID của zone.</param>
    /// <returns>Chi tiết zone.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetZoneById(int id)
    {
        var result = await _zoneService.GetZoneByIdAsync(id);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy tất cả zone.
    /// </summary>
    /// <returns>Danh sách tất cả zone.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllZones()
    {
        var result = await _zoneService.GetAllZonesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Lấy zone cho một floor cụ thể.
    /// </summary>
    /// <param name="floorId">ID floor.</param>
    /// <returns>Zone trong floor đã chỉ định.</returns>
    [HttpGet("floor/{floorId}")]
    public async Task<IActionResult> GetZonesByFloor(int floorId)
    {
        var result = await _zoneService.GetZonesByFloorAsync(floorId);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Lấy zone theo phân trang.
    /// </summary>
    /// <param name="pageIndex">Chỉ số trang (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số mục mỗi trang.</param>
    /// <returns>Danh sách zone phân trang.</returns>
    [HttpGet("paged")]
    public async Task<IActionResult> GetZonesPaged([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        if (pageIndex < 1 || pageSize < 1)
            return BadRequest("Page index and page size must be greater than 0.");

        var result = await _zoneService.GetZonesPagedAsync(pageIndex, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing zone.
    /// </summary>
    /// <param name="id">The zone identifier.</param>
    /// <param name="request">The zone update request.</param>
    /// <returns>The updated zone.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateZone(int id, [FromBody] ZoneUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _zoneService.UpdateZoneAsync(id, request);
        
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Deletes a zone.
    /// </summary>
    /// <param name="id">The zone identifier.</param>
    /// <returns>Success or error response.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteZone(int id)
    {
        var result = await _zoneService.DeleteZoneAsync(id);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
