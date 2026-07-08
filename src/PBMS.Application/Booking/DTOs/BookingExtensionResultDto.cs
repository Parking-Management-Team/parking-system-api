using System;

namespace PBMS.Application.Booking.DTOs
{
    /// <summary>
    /// DTO chứa kết quả yêu cầu gia hạn đặt chỗ.
    /// </summary>
    public class BookingExtensionResultDto
    {
        public bool Success { get; set; }
        public bool IsCapped { get; set; }
        public DateTime AdjustedEndTime { get; set; }
        public decimal AdditionalFee { get; set; }
        public int? PaymentId { get; set; }
        public string Message { get; set; } = null!;
        public string? PaymentUrl { get; set; }
    }
}
