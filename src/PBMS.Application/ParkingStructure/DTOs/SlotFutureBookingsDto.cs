using System.Collections.Generic;
using PBMS.Application.Booking.DTOs;

namespace PBMS.Application.ParkingStructure.DTOs;

/// <summary>
/// DTO chứa danh sách booking tương lai và các slot đỗ khuyến nghị thay thế.
/// </summary>
public class SlotFutureBookingsDto
{
    public int SlotId { get; set; }
    public string SlotCode { get; set; } = null!;
    public List<BookingDto> FutureBookings { get; set; } = new();
    public List<RecommendedSlotDto> RecommendedSlots { get; set; } = new();
}

/// <summary>
/// DTO mô tả chi tiết của một slot đỗ khuyến nghị thay thế.
/// </summary>
public class RecommendedSlotDto
{
    public int SlotId { get; set; }
    public string SlotCode { get; set; } = null!;
    public int FutureBookingCount { get; set; }
    public string Message { get; set; } = null!;
}
