# 📋 Kế Hoạch Triển Khai: Tính Năng Booking Reservation

> **Branch:** `feature/booking-reservation`
> **Ngày:** 20/06/2026
> **Mục tiêu:** Driver có thể đặt chỗ trước, thanh toán tiền cọc, và hệ thống giữ General Capacity cho họ.

---

## 📌 Tổng Quan Luồng Nghiệp Vụ

```
Driver gửi request
       │
       ▼
[POST /api/bookings]
       │
       ├─ Validate thời gian (1h ~ 8h tính từ hiện tại)
       ├─ Kiểm tra General Capacity còn không?
       ├─ Tính Deposit Fee (= BasePrice của PricingWindow tại giờ checkin dự kiến)
       ├─ Tạo Booking (status = PENDING)
       │
       ▼
[POST /api/payments]  ← Driver thanh toán tiền cọc (CASH hoặc ONLINE_BANKING)
       │
       ├─ Nếu ONLINE_BANKING: VNPay gọi callback IPN
       │
       ▼
[CompleteBusinessFlowAsync]  ← Đã có sẵn trong PaymentService!
       │
       └─ Booking.BookingStatus → "Confirmed"
          Booking.ConfirmedAt  → DateTime.UtcNow
```

> ✅ **Điểm quan trọng:** Luồng `Booking PENDING → CONFIRMED` sau khi thanh toán **đã được code sẵn** trong `PaymentService.CompleteBusinessFlowAsync()`. Chúng ta chỉ cần xây dựng phần **tạo Booking**.

---

## 🏗️ Kiến Trúc Tổng Thể (Đã Có Sẵn Trong Dự Án)

```
PBMS.API                   ← HTTP Controllers (endpoint)
    │
PBMS.Application           ← Business Logic (Services, DTOs, Interfaces)
    │
PBMS.Domain                ← Entities, Enums, Business Rules
    │
PBMS.Infrastructure        ← Database (Repositories, EF Core, Migrations)
```

**Quy tắc đặt tên theo convention của dự án:**
- `Booking/DTOs/` → Chứa Request/Response DTO
- `Booking/Interfaces/` → Chứa `IBookingService`
- `Booking/Services/` → Chứa `BookingService` (logic thực tế)

---

## 📊 Phân Tích Entity & Quan Hệ Database

### Booking Entity (đã có tại `PBMS.Domain/Entities/Booking.cs`)

| Field | Kiểu | Mô tả |
|-------|------|-------|
| `Id` | `int` | PK (kế thừa BaseEntity) |
| `AccountId` | `int` | FK → Account |
| `VehicleId` | `int` | FK → Vehicle |
| `VehicleTypeId` | `int` | FK → VehicleType (snapshot lúc đặt) |
| `BuildingId` | `int` | FK → Building |
| `PlannedCheckinTime` | `DateTime` | Giờ dự kiến vào bãi |
| `PlannedCheckoutTime` | `DateTime` | Giờ dự kiến ra (chưa dùng trong scope này) |
| `DepositAmount` | `decimal` | Tiền cọc = BasePrice của block đầu |
| `BookingStatus` | `string` | `"Pending"` → `"Confirmed"` |
| `PaymentDeadline` | `DateTime` | Hạn thanh toán cọc |
| `CheckinGraceUntil` | `DateTime` | Hạn grace period check-in sau giờ dự kiến |
| `ConfirmedAt` | `DateTime?` | Thời điểm xác nhận |

### Mối Quan Hệ Quan Trọng

```
Building ──┬──< Floor ──< Zone (AccessType: General/Monthly)
           │                └──< ParkingSlot
           │
Booking ───┼── AccountId
           ├── VehicleId  ──> Vehicle ──> VehicleType
           ├── BuildingId
           └── Payments (1 booking có thể có nhiều payment)

ParkingSession ──> BookingId (nullable) ← liên kết khi xe thực sự check-in
```

### Quy Tắc General Capacity

**General Capacity** của Building cho một `VehicleTypeId` = Tổng `capacity` của tất cả Zone có:
- `AccessType = General`  
- `VehicleTypeId = loại xe cần kiểm tra`
- Zone thuộc Floor thuộc Building đó

**Số chỗ đã dùng** = Số `ParkingSession` đang `ACTIVE` + Số `Booking` đang `PENDING` hoặc `Confirmed` (chưa check-in) tại cùng Building + VehicleType.

---

## ✅ Danh Sách Task Cần Làm (Theo Thứ Tự)

---

### 🟡 TASK 1 — Thêm BookingStatus Enum
**Layer:** `PBMS.Domain`  
**File mới:** `src/PBMS.Domain/Enums/BookingStatus.cs`

**Lý do:** Hiện tại `BookingStatus` đang là `string` thuần trong entity. Cần tạo enum tương tự `SessionStatus` để code rõ ràng, tránh magic string.

```csharp
// src/PBMS.Domain/Enums/BookingStatus.cs
namespace PBMS.Domain.Enums;

public static class BookingStatus
{
    /// Booking đã tạo, chờ thanh toán cọc
    public const string Pending = "Pending";

    /// Đã thanh toán cọc, chỗ được giữ chắc chắn
    public const string Confirmed = "Confirmed";

    /// Driver đã check-in, booking hoàn thành vai trò
    public const string CheckedIn = "CheckedIn";

    /// Booking bị hủy (timeout hoặc user hủy)
    public const string Cancelled = "Cancelled";

    /// Driver không đến trong thời gian cho phép
    public const string NoShow = "NoShow";

    /// Expired — đã qua PaymentDeadline mà chưa thanh toán
    public const string Expired = "Expired";
}
```

**Thời gian ước tính:** ~15 phút

---

### 🟡 TASK 2 — Thêm Method Kiểm Tra Capacity Vào IBuildingRepository
**Layer:** `PBMS.Application` (Contracts)  
**File sửa:** `src/PBMS.Application/Contracts/IBuildingRepository.cs`

**Lý do:** Hiện tại `IBuildingRepository` chỉ có `GetTotalMotorcycleCapacityAsync`. Cần thêm method tổng quát để lấy tổng General Capacity theo loại xe.

```csharp
// Thêm vào IBuildingRepository.cs

/// <summary>
/// Lấy tổng General Capacity của Building cho một loại xe cụ thể.
/// Tính bằng tổng capacity của các Zone có AccessType = General và VehicleTypeId khớp.
/// </summary>
Task<int> GetTotalGeneralCapacityAsync(int buildingId, int vehicleTypeId);
```

**Thời gian ước tính:** ~10 phút

---

### 🟡 TASK 3 — Thêm IBookingRepository (Contracts)
**Layer:** `PBMS.Application` (Contracts)  
**File mới:** `src/PBMS.Application/Contracts/IBookingRepository.cs`

**Lý do:** Cần interface để BookingService gọi, và Infrastructure implement. Pattern giống `IMonthlySubscriptionRepository`.

```csharp
// src/PBMS.Application/Contracts/IBookingRepository.cs
using PBMS.Domain.Entities;

namespace PBMS.Application.Contracts;

public interface IBookingRepository : IRepository<Booking>
{
    /// <summary>
    /// Đếm số Booking PENDING + CONFIRMED (đang giữ chỗ) tại Building cho loại xe,
    /// trong khoảng thời gian checkin dự kiến. Dùng để tính capacity đã bị khóa.
    /// </summary>
    Task<int> GetActiveBookingsCountAsync(int buildingId, int vehicleTypeId, DateTime plannedCheckinTime);
}
```

**Thời gian ước tính:** ~15 phút

---

### 🟡 TASK 4 — Tạo DTOs Cho Booking
**Layer:** `PBMS.Application`  
**Files mới:**
- `src/PBMS.Application/Booking/DTOs/CreateBookingRequest.cs`
- `src/PBMS.Application/Booking/DTOs/BookingDto.cs`

```csharp
// CreateBookingRequest.cs — Dữ liệu Driver gửi lên
public class CreateBookingRequest
{
    public int AccountId { get; set; }          // ID tài khoản driver
    public string LicensePlate { get; set; }    // Biển số xe (hệ thống tự tìm VehicleId)
    public int BuildingId { get; set; }         // Building muốn đặt
    public DateTime PlannedCheckinTime { get; set; } // Giờ dự kiến vào (1h-8h từ hiện tại)
}

// BookingDto.cs — Dữ liệu trả về cho client
public class BookingDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string LicensePlate { get; set; }
    public int BuildingId { get; set; }
    public string BuildingName { get; set; }
    public DateTime PlannedCheckinTime { get; set; }
    public decimal DepositAmount { get; set; }
    public string BookingStatus { get; set; }
    public DateTime PaymentDeadline { get; set; }
    public DateTime CheckinGraceUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

**Thời gian ước tính:** ~20 phút

---

### 🟡 TASK 5 — Tạo IBookingService Interface
**Layer:** `PBMS.Application`  
**File mới:** `src/PBMS.Application/Booking/Interfaces/IBookingService.cs`

```csharp
// IBookingService.cs
using PBMS.Application.Booking.DTOs;

namespace PBMS.Application.Booking.Interfaces;

public interface IBookingService
{
    /// <summary>
    /// Tạo đặt chỗ mới (trạng thái PENDING).
    /// Kiểm tra thời gian hợp lệ (1h-8h), capacity, tính deposit fee.
    /// </summary>
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);

    /// <summary>
    /// Lấy thông tin Booking theo ID.
    /// </summary>
    Task<BookingDto> GetBookingByIdAsync(int id);

    /// <summary>
    /// Hủy booking (trạng thái → Cancelled).
    /// </summary>
    Task CancelBookingAsync(int id, string reason);
}
```

**Thời gian ước tính:** ~15 phút

---

### 🔴 TASK 6 — Triển Khai BookingService (Core Logic)
**Layer:** `PBMS.Application` ← **Task quan trọng nhất**  
**File mới:** `src/PBMS.Application/Booking/Services/BookingService.cs`

**Chi tiết logic `CreateBookingAsync`:**

```
Bước 1: Validate thời gian
────────────────────────────
- plannedCheckinTime phải từ 1h đến 8h sau DateTime.UtcNow
- Nếu vi phạm → throw DomainException("INVALID_BOOKING_TIME", ...)

Bước 2: Tìm xe theo biển số
────────────────────────────
- Gọi VehicleRepository.FindAsync(v => v.LicensePlate == request.LicensePlate)
- Nếu không tìm thấy → throw DomainException("VEHICLE_NOT_FOUND", ...)
- Lấy VehicleTypeId từ xe

Bước 3: Kiểm tra Building tồn tại
────────────────────────────
- BuildingRepository.GetByIdAsync(request.BuildingId)
- Nếu không tồn tại → throw DomainException("BUILDING_NOT_FOUND", ...)

Bước 4: Kiểm tra General Capacity
────────────────────────────
- totalCapacity = BuildingRepository.GetTotalGeneralCapacityAsync(buildingId, vehicleTypeId)
- activeCount   = ActiveParkingSessions tại Building (SessionStatus = ACTIVE)
                + ActiveBookings tại Building (Status = PENDING | Confirmed)
- Nếu activeCount >= totalCapacity → throw DomainException("NO_CAPACITY", ...)

Bước 5: Tính Deposit Fee
────────────────────────────
- Tìm PricingPolicy Active cho VehicleTypeId tại PlannedCheckinTime
- Tìm PricingWindow khớp với giờ PlannedCheckinTime (theo StartTime - EndTime)
- depositFee = window.BasePrice

Bước 6: Tạo Booking
────────────────────────────
- BookingStatus = "Pending"
- PaymentDeadline = DateTime.UtcNow + 15 phút (thời gian chờ thanh toán)
- CheckinGraceUntil = PlannedCheckinTime + 30 phút (grace period check-in)

Bước 7: Lưu & trả về DTO
```

**Dependencies cần inject:**
```csharp
public BookingService(
    IBookingRepository bookingRepository,
    IRepository<Vehicle> vehicleRepository,
    IRepository<Building> buildingRepository,
    IBuildingRepository buildingDetailRepository,
    IPricingPolicyRepository pricingPolicyRepository,
    IRepository<ParkingSession> sessionRepository,
    IUnitOfWork unitOfWork)
```

**Thời gian ước tính:** ~90 phút

---

### 🟡 TASK 7 — Triển Khai BookingRepository
**Layer:** `PBMS.Infrastructure`  
**File mới:** `src/PBMS.Infrastructure/Repositories/BookingRepository.cs`

```csharp
// BookingRepository.cs
public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context) { }

    public async Task<int> GetActiveBookingsCountAsync(
        int buildingId, int vehicleTypeId, DateTime plannedCheckinTime)
    {
        // Đếm booking đang "chiếm" chỗ tại building + loại xe
        // Điều kiện: Status là Pending hoặc Confirmed
        // (Optional: lọc theo khoảng thời gian overlap nếu cần)
        return await _dbSet
            .Include(b => b.Vehicle)
            .CountAsync(b =>
                b.BuildingId == buildingId &&
                b.Vehicle.VehicleTypeId == vehicleTypeId &&
                (b.BookingStatus == "Pending" || b.BookingStatus == "Confirmed"));
    }
}
```

**Thời gian ước tính:** ~30 phút

---

### 🟡 TASK 8 — Implement BuildingRepository.GetTotalGeneralCapacityAsync
**Layer:** `PBMS.Infrastructure`  
**File sửa:** `src/PBMS.Infrastructure/Repositories/BuildingRepository.cs`

```csharp
// Thêm vào BuildingRepository.cs
public async Task<int> GetTotalGeneralCapacityAsync(int buildingId, int vehicleTypeId)
{
    // Zone → Floor → Building, lọc AccessType = General
    return await _context.Zones
        .Include(z => z.Floor)
        .Where(z =>
            z.Floor.BuildingId == buildingId &&
            z.VehicleTypeId == vehicleTypeId &&
            z.AccessType == ZoneAccessType.General &&
            z.Status == ZoneStatus.Available)
        .SumAsync(z => z.Capacity);
}
```

**Thời gian ước tính:** ~20 phút

---

### 🟡 TASK 9 — Đăng Ký Trong DependencyInjection
**Layer:** `PBMS.Application` + `PBMS.Infrastructure`

**Sửa** `src/PBMS.Application/DependencyInjection.cs`:
```csharp
// Thêm vào cuối phần đăng ký
services.AddScoped<IBookingService, BookingService>();
```

**Sửa** `src/PBMS.Infrastructure/DependencyInjection.cs`:
```csharp
// Thêm repository
services.AddScoped<IBookingRepository, BookingRepository>();
```

**Thời gian ước tính:** ~10 phút

---

### 🟡 TASK 10 — Tạo BookingsController (API Endpoint)
**Layer:** `PBMS.API`  
**File mới:** `src/PBMS.API/Controllers/BookingsController.cs`

```csharp
[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    // POST /api/bookings — Tạo đặt chỗ mới
    [HttpPost]
    public async Task<ActionResult<BaseResponse<BookingDto>>> CreateBooking(
        [FromBody] CreateBookingRequest request)
    {
        var booking = await _bookingService.CreateBookingAsync(request);
        return CreatedAtAction(
            nameof(GetBookingById),
            new { id = booking.Id },
            BaseResponse<BookingDto>.Ok(booking, "Booking created successfully."));
    }

    // GET /api/bookings/{id} — Lấy thông tin booking
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BaseResponse<BookingDto>>> GetBookingById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        return Ok(BaseResponse<BookingDto>.Ok(booking));
    }

    // DELETE /api/bookings/{id} — Hủy booking
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<BaseResponse<string>>> CancelBooking(
        int id, [FromQuery] string reason = "User cancelled")
    {
        await _bookingService.CancelBookingAsync(id, reason);
        return Ok(BaseResponse<string>.Ok("Booking cancelled successfully."));
    }
}
```

**Thời gian ước tính:** ~30 phút

---

## 🔄 Luồng Xác Nhận Booking (Đã Có Sẵn — Không Cần Làm Thêm)

Khi Driver thanh toán tiền cọc xong, `PaymentService.CompleteBusinessFlowAsync()` **đã được implement** sẵn tại dòng 292-303:

```csharp
else if (payment.BookingId.HasValue)
{
    // Thanh toán đặt cọc → Xác nhận đặt cọc thành công
    var booking = await _bookingRepository.GetByIdAsync(payment.BookingId.Value);
    if (booking != null)
    {
        booking.BookingStatus = "Confirmed";   // ← Scenario 2: AC đã được thỏa!
        booking.ConfirmedAt = DateTime.UtcNow;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();
    }
}
```

**Driver chỉ cần gọi:** `POST /api/payments` với `{ "BookingId": <id>, "PaymentMethod": "ONLINE_BANKING" }`

---

## 📐 Sơ Đồ Luồng Đầy Đủ

```
[Driver] ──POST /api/bookings──► [BookingsController]
                                        │
                                 [BookingService.CreateBookingAsync]
                                        │
                         ┌──────────────┴──────────────────┐
                         │                                  │
                 Validate thời gian                 Kiểm tra capacity
                 (1h ≤ t ≤ 8h)                    (General Zone)
                         │                                  │
                         └──────────────┬──────────────────┘
                                        │
                                Tính Deposit Fee
                                (PricingWindow.BasePrice)
                                        │
                                Tạo Booking (PENDING)
                                        │
                                Return BookingDto
                                   (có DepositAmount)
                                        │
[Driver] ──POST /api/payments──►  [PaymentService]
         { BookingId, Method }           │
                                   PaymentStatus = PAID
                                        │
                               CompleteBusinessFlowAsync
                                        │
                               BookingStatus = "Confirmed" ✅
```

---

## 📁 Tổng Hợp Files Cần Tạo / Sửa

| Hành động | File | Task |
|-----------|------|------|
| 🆕 Tạo mới | `PBMS.Domain/Enums/BookingStatus.cs` | Task 1 |
| ✏️ Sửa | `PBMS.Application/Contracts/IBuildingRepository.cs` | Task 2 |
| 🆕 Tạo mới | `PBMS.Application/Contracts/IBookingRepository.cs` | Task 3 |
| 🆕 Tạo mới | `PBMS.Application/Booking/DTOs/CreateBookingRequest.cs` | Task 4 |
| 🆕 Tạo mới | `PBMS.Application/Booking/DTOs/BookingDto.cs` | Task 4 |
| 🆕 Tạo mới | `PBMS.Application/Booking/Interfaces/IBookingService.cs` | Task 5 |
| 🆕 Tạo mới | `PBMS.Application/Booking/Services/BookingService.cs` | Task 6 |
| 🆕 Tạo mới | `PBMS.Infrastructure/Repositories/BookingRepository.cs` | Task 7 |
| ✏️ Sửa | `PBMS.Infrastructure/Repositories/BuildingRepository.cs` | Task 8 |
| ✏️ Sửa | `PBMS.Application/DependencyInjection.cs` | Task 9 |
| ✏️ Sửa | `PBMS.Infrastructure/DependencyInjection.cs` | Task 9 |
| 🆕 Tạo mới | `PBMS.API/Controllers/BookingsController.cs` | Task 10 |

---

## ⚠️ Lưu Ý & Quyết Định Thiết Kế

### 1. Không cần Migration mới
Entity `Booking` đã tồn tại đầy đủ trong database và codebase. Không cần thêm column hay migration.

### 2. Deposit Fee = BasePrice của PricingWindow tại giờ dự kiến check-in
- Tra cứu `PricingPolicy` Active theo `VehicleTypeId`
- Tìm `PricingWindow` có `StartTime ≤ plannedCheckinTime.TimeOfDay < EndTime`
- `DepositFee = window.BasePrice`

### 3. Tính General Capacity — Chiến lược đếm chỗ
Công thức: `Chỗ trống = TotalCapacity - ActiveSessions - ActiveBookings`
- **ActiveSessions**: ParkingSession có `SessionStatus = ACTIVE` tại Building + VehicleType
- **ActiveBookings**: Booking có `Status = Pending | Confirmed` tại Building + VehicleType

### 4. PaymentDeadline & CheckinGraceUntil
- `PaymentDeadline = Now + 15 phút` (nếu quá hạn → job tự động expire)
- `CheckinGraceUntil = PlannedCheckinTime + 30 phút` (nếu quá hạn mà chưa check-in → NoShow)

### 5. Confirm Booking — Không cần code thêm
`PaymentService.CompleteBusinessFlowAsync()` tại dòng 292-303 đã xử lý đúng.

---

## 🔢 Thứ Tự Thực Hiện (Recommended)

```
Task 1  →  Task 2  →  Task 3  →  Task 4  →  Task 5
  (Enum)    (IBuildRepo)  (IBookRepo)  (DTOs)   (Interface)
                                                    │
                                                    ▼
                                              Task 6  ←─── Task quan trọng nhất
                                             (Service)
                                                    │
                                    ┌───────────────┤
                                    ▼               ▼
                                  Task 7          Task 8
                               (BookingRepo)  (BuildingRepo)
                                    │               │
                                    └───────┬───────┘
                                            ▼
                                          Task 9
                                            │
                                            ▼
                                          Task 10
                                        (Controller)
```

---

## 🧪 Kiểm Thử (Test Cases)

### Test 1 — Tạo Booking thành công
```
POST /api/bookings
{
  "accountId": 3,
  "licensePlate": "51G-12345",
  "buildingId": 1,
  "plannedCheckinTime": "2026-06-20T20:00:00Z"  // 3h sau hiện tại
}

Expected Response 201:
{
  "data": {
    "id": 1,
    "bookingStatus": "Pending",
    "depositAmount": 20000,  // BasePrice ô tô ca ngày
    "paymentDeadline": "2026-06-20T17:04:xx"
  }
}
```

### Test 2 — Thời gian không hợp lệ (< 1h)
```
POST /api/bookings
{
  "plannedCheckinTime": "2026-06-20T17:50:00Z"  // chỉ 1 phút sau
}

Expected Response 400:
{ "error": "INVALID_BOOKING_TIME", "message": "Thời gian đặt chỗ phải từ 1 đến 8 tiếng từ thời điểm hiện tại." }
```

### Test 3 — Xác nhận sau thanh toán
```
POST /api/payments
{
  "bookingId": 1,
  "paymentMethod": "CASH"
}

Expected: Booking.BookingStatus → "Confirmed" ✅
```

---

*Tài liệu này được tạo tự động dựa trên phân tích codebase và SRS requirements.*
