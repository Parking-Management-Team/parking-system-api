using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// Request DTO cho việc thay đổi trạng thái slot (Block/Unblock/Maintenance).
/// </summary>
public class SlotStatusChangeRequest
{
    /// <summary>
    /// Lý do thay đổi trạng thái.
    /// </summary>
    [StringLength(500)]
    public string? Reason { get; set; }
}