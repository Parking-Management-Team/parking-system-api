using System.Text.Json;

namespace PBMS.Domain.Entities;

/// <summary>
/// Thực thể ghi nhật ký chi tiết tính toán phí (PricingCalculationLog) dùng để đối soát.
/// Kế thừa từ BaseEntity (Id, CreatedAt, RowVersion).
/// </summary>
public class PricingCalculationLog : BaseEntity
{
    /// <summary>
    /// ID của lượt đặt chỗ (nếu có).
    /// </summary>
    public int? BookingId { get; set; }

    /// <summary>
    /// ID của lượt gửi xe (ParkingSessionId).
    /// </summary>
    public int? ParkingSessionId { get; set; }

    /// <summary>
    /// ID thanh toán tương ứng (nếu có).
    /// </summary>
    public int? PaymentId { get; set; }

    /// <summary>
    /// Mục đích tính toán phí: BOOKING_DEPOSIT, SESSION_EXTENSION, CHECKOUT_FINAL.
    /// </summary>
    public string CalculationPurpose { get; set; } = "CHECKOUT_FINAL";

    /// <summary>
    /// Mã tương quan / idempotency key phục vụ chống trùng lặp log đối soát.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// ID loại phương tiện.
    /// </summary>
    public int VehicleTypeId { get; set; }

    /// <summary>
    /// Thời điểm xe vào bãi / bắt đầu khoảng tính phí.
    /// </summary>
    public DateTime CheckInTime { get; set; }

    /// <summary>
    /// Thời điểm xe ra bãi / kết thúc khoảng tính phí.
    /// </summary>
    public DateTime CheckOutTime { get; set; }

    /// <summary>
    /// ID chính sách giá đã khớp.
    /// </summary>
    public int MatchedPolicyId { get; set; }

    /// <summary>
    /// Tổng phí tính toán được.
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Chi tiết các bước tính toán theo định dạng JSON string.
    /// </summary>
    public string CalculationDetails { get; set; } = null!;

    // -----------------------------------------------------------------------
    // NAVIGATION PROPERTIES
    // -----------------------------------------------------------------------

    /// <summary>
    /// Giao dịch thanh toán liên quan (nếu có).
    /// </summary>
    public virtual Payment? Payment { get; set; }

    /// <summary>
    /// Loại phương tiện áp dụng.
    /// </summary>
    public virtual VehicleType VehicleType { get; set; } = null!;

    /// <summary>
    /// Chính sách giá đã được áp dụng.
    /// </summary>
    public virtual PricingPolicy MatchedPolicy { get; set; } = null!;
}
