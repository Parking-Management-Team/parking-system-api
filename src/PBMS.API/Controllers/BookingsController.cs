using Microsoft.AspNetCore.Mvc;
using PBMS.Application.Booking.DTOs;
using PBMS.Application.Booking.Interfaces;
using PBMS.Application.Common;

namespace PBMS.API.Controllers;

/// <summary>
/// API Controller quản lý Đặt chỗ trước (Booking Reservation).
/// Endpoint chính: /api/bookings
///
/// Actor: Driver (khách hàng muốn đặt chỗ trước)
///
/// Các chức năng CRUD:
///   POST   /api/bookings                              → Tạo Booking mới (PENDING)
///   GET    /api/bookings                              → Lấy tất cả Booking (Admin/Staff)
///   GET    /api/bookings/{id}                         → Lấy chi tiết Booking theo ID
///   GET    /api/bookings/by-account/{accountId}       → Lấy Booking theo Account
///   GET    /api/bookings/by-building/{buildingId}     → Lấy Booking theo Building
///   PUT    /api/bookings/{id}                         → Cập nhật thời gian đặt chỗ (PENDING only)
///   DELETE /api/bookings/{id}                         → Hủy Booking
///   POST   /api/bookings/cleanup                      → Dọn dẹp Booking hết hạn (background job)
/// </summary>
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Constructor nhận IBookingService qua Dependency Injection.
    /// </summary>
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
    }

    // -----------------------------------------------------------------------
    // POST /api/bookings — Tạo Booking mới
    // -----------------------------------------------------------------------

    /// <summary>
    /// Tạo đặt chỗ trước mới cho Driver.
    ///
    /// Route  : POST /api/bookings
    /// Body   : CreateBookingRequest (AccountId, LicensePlate, BuildingId, PlannedCheckinTime)
    /// Returns: 201 Created + BookingDto nếu thành công
    ///          400 Bad Request nếu thời gian không hợp lệ
    ///          404 Not Found nếu xe hoặc Building không tồn tại
    ///          409 Conflict nếu Building không còn capacity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<BookingDto>>> CreateBooking(
        [FromBody] CreateBookingRequest request)
    {
        var booking = await _bookingService.CreateBookingAsync(request);

        return CreatedAtAction(
            actionName: nameof(GetBookingById),
            routeValues: new { id = booking.Id },
            value: BaseResponse<BookingDto>.Ok(booking, "Đặt chỗ được tạo thành công. Vui lòng thanh toán tiền cọc trong vòng 15 phút.")
        );
    }

    // -----------------------------------------------------------------------
    // GET /api/bookings — Lấy tất cả Booking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách toàn bộ Booking trong hệ thống (dành cho Admin/Staff).
    ///
    /// Route  : GET /api/bookings
    /// Returns: 200 OK + Danh sách BookingDto
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<BookingDto>>>> GetAllBookings()
    {
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(BaseResponse<List<BookingDto>>.Ok(bookings));
    }

    // -----------------------------------------------------------------------
    // GET /api/bookings/{id} — Lấy chi tiết Booking theo ID
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy thông tin chi tiết Booking theo ID.
    ///
    /// Route  : GET /api/bookings/{id}
    /// Returns: 200 OK + BookingDto nếu tìm thấy
    ///          404 Not Found nếu không có Booking với ID này
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<BookingDto>>> GetBookingById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        return Ok(BaseResponse<BookingDto>.Ok(booking));
    }

    // -----------------------------------------------------------------------
    // GET /api/bookings/by-account/{accountId} — Lấy Booking theo Account
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách Booking của một Driver cụ thể.
    ///
    /// Route  : GET /api/bookings/by-account/{accountId}
    /// Returns: 200 OK + Danh sách BookingDto của Account đó
    /// </summary>
    [HttpGet("by-account/{accountId:int}")]
    public async Task<ActionResult<BaseResponse<List<BookingDto>>>> GetBookingsByAccount(int accountId)
    {
        var bookings = await _bookingService.GetBookingsByAccountIdAsync(accountId);
        return Ok(BaseResponse<List<BookingDto>>.Ok(bookings));
    }

    // -----------------------------------------------------------------------
    // GET /api/bookings/by-building/{buildingId} — Lấy Booking theo Building
    // -----------------------------------------------------------------------

    /// <summary>
    /// Lấy danh sách Booking tại một Tòa nhà cụ thể (dành cho Staff quản lý bãi).
    ///
    /// Route  : GET /api/bookings/by-building/{buildingId}
    /// Returns: 200 OK + Danh sách BookingDto của Building đó
    /// </summary>
    [HttpGet("by-building/{buildingId:int}")]
    public async Task<ActionResult<BaseResponse<List<BookingDto>>>> GetBookingsByBuilding(int buildingId)
    {
        var bookings = await _bookingService.GetBookingsByBuildingIdAsync(buildingId);
        return Ok(BaseResponse<List<BookingDto>>.Ok(bookings));
    }

    // -----------------------------------------------------------------------
    // PUT /api/bookings/{id} — Cập nhật thời gian đặt chỗ
    // -----------------------------------------------------------------------

    /// <summary>
    /// Cập nhật thời gian check-in dự kiến của Booking.
    /// Chỉ cho phép cập nhật khi Booking đang ở trạng thái PENDING.
    /// Deposit Fee sẽ được tính lại theo giờ mới.
    ///
    /// Route  : PUT /api/bookings/{id}
    /// Body   : UpdateBookingRequest (PlannedCheckinTime mới)
    /// Returns: 200 OK + BookingDto sau khi cập nhật
    ///          400 Bad Request nếu thời gian không hợp lệ
    ///          404 Not Found nếu không tìm thấy Booking
    ///          409 Conflict nếu Booking không ở trạng thái PENDING
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BaseResponse<BookingDto>>> UpdateBooking(
        int id,
        [FromBody] UpdateBookingRequest request)
    {
        var booking = await _bookingService.UpdateBookingAsync(id, request);
        return Ok(BaseResponse<BookingDto>.Ok(booking, "Cập nhật thời gian đặt chỗ thành công."));
    }

    // -----------------------------------------------------------------------
    // DELETE /api/bookings/{id} — Hủy Booking
    // -----------------------------------------------------------------------

    /// <summary>
    /// Hủy Booking (chuyển trạng thái sang Cancelled).
    /// Chỉ hủy được khi Booking đang Pending hoặc Confirmed.
    /// Capacity tạm giữ sẽ được giải phóng ngay khi hủy.
    ///
    /// Route  : DELETE /api/bookings/{id}
    /// Query  : reason (tùy chọn) — lý do hủy
    /// Returns: 204 No Content nếu hủy thành công
    ///          404 Not Found nếu không tìm thấy Booking
    ///          409 Conflict nếu Booking không thể hủy (đã CheckedIn/NoShow/Expired)
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> CancelBooking(
        int id,
        [FromQuery] string? reason = null)
    {
        await _bookingService.CancelBookingAsync(id, reason);
        return NoContent();
    }

    // -----------------------------------------------------------------------
    // POST /api/bookings/cleanup — Dọn dẹp Booking hết hạn
    // -----------------------------------------------------------------------

    /// <summary>
    /// Dọn dẹp các Booking PENDING đã hết hạn PaymentDeadline → chuyển sang Expired.
    /// Thường được gọi bởi background job định kỳ (hoặc gọi thủ công bởi Admin).
    ///
    /// Route  : POST /api/bookings/cleanup
    /// Returns: 200 OK + thông báo số lượng Booking đã được xử lý
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<BaseResponse<string>>> CleanupExpiredBookings()
    {
        await _bookingService.CleanupExpiredPendingBookingsAsync();
        return Ok(BaseResponse<string>.Ok("Dọn dẹp các Booking hết hạn thành công."));
    }
}
