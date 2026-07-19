using System.ComponentModel.DataAnnotations;

namespace PBMS.Application.ParkingSession.DTOs;

public class UpdateCheckinRequest
{
    [MaxLength(20)]
    public string? LicensePlate { get; set; }

    public int? VehicleTypeId { get; set; }

    [MaxLength(20)]
    public string? CardCode { get; set; }

    public int? ZoneId { get; set; }

    public int? SlotId { get; set; }
}
