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
        /// <summary>Phí của booking gốc (trước khi extend), dùng để hiển thị fee breakdown.</summary>
        public decimal OriginalFee { get; set; }
        /// <summary>Tổng phí mới sau khi extend = OriginalFee + AdditionalFee.</summary>
        public decimal NewTotalFee { get; set; }
        /// <summary>Thời điểm tối đa có thể extend (tính từ buffer của booking tiếp theo). MinValue = không bị giới hạn.</summary>
        public DateTime MaxAllowedEndTime { get; set; }
        /// <summary>Số tiền phạt quá giờ (nếu có), dùng để hiển thị riêng trên bill hóa đơn bước 2.</summary>
        public decimal PenaltyFee { get; set; }
        public int? PaymentId { get; set; }
        public string Message { get; set; } = null!;
        public string? PaymentUrl { get; set; }
    }
}
