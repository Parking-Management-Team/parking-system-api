using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

public class StartCheckoutRequest
{
    public DateTime? CheckOutTime { get; set; }

    [MaxLength(20)]
    public string? LicensePlateOut { get; set; }

    public int? OutStaffId { get; set; }
}
