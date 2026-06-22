using System;

namespace PBMS.Application.Blacklist.DTOs;

/// <summary>
/// DTO trả về thông tin danh sách đen (Blacklist).
/// </summary>
public class BlacklistDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? LicensePlate { get; set; } // Lấy từ thực thể Vehicle
    public int? CardId { get; set; }
    public string? CardCode { get; set; } // Lấy từ thực thể Card
    public int? IncidentId { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
