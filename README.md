# PBMS Backend — Parking Building Management System

> **Công nghệ:** ASP.NET Core Web API · Entity Framework Core · C# · Postgresql
> **Kiến trúc:** Clean Architecture (4-layer)

---

## Mục lục

- [Tổng quan kiến trúc](#tổng-quan-kiến-trúc)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Phân chia domain](#phân-chia-domain)
- [Quy ước đặt tên](#quy-ước-đặt-tên)
- [Hướng dẫn bắt đầu](#hướng-dẫn-bắt-đầu)
- [Quy trình làm việc nhóm (Git workflow)](#quy-trình-làm-việc-nhóm-git-workflow)
- [Phân công domain](#phân-công-domain)

---

## Tổng quan kiến trúc

Project được tổ chức theo **Clean Architecture**, chia thành 4 project con trong cùng một solution:

```
PBMS.sln
├── src/
│   ├── PBMS.API              ← Tiếp nhận request, trả response (Controllers, Middleware)
│   ├── PBMS.Application      ← Logic nghiệp vụ (Services, DTOs, Interfaces)
│   ├── PBMS.Domain           ← Entity, Enum, Interface repository (core nhất, ít thay đổi)
│   └── PBMS.Infrastructure   ← Database, Repository, EF Migrations, External Services
└── tests/
    └── PBMS.UnitTests        ← Unit test cho các service
```

**Dependency rule:** Các tầng chỉ được phép phụ thuộc vào tầng bên trong, không được phụ thuộc ra ngoài.

```
API  →  Application  →  Domain
         ↑
Infrastructure  →  Domain
```

- `PBMS.Domain` — thuần túy nhất: chỉ chứa Entity, Enum, Exception nghiệp vụ. Không phụ thuộc gì cả.
- `PBMS.Application` — định nghĩa Interface repository (`IBookingRepository`, `IUnitOfWork`...) và Service. Infrastructure sẽ implement các interface này.
- `PBMS.Infrastructure` — implement interface từ `Application`, không để Application biết gì về EF Core hay DB.
- `PBMS.API` — chỉ gọi Service từ Application, không chứa business logic.

---

## Dependency Injection (DI)

- Mỗi tầng có một file riêng `DependencyInjection.cs` để đăng ký các dịch vụ của chính tầng đó.
- `PBMS.Application/DependencyInjection.cs` đăng ký application services, use case, validator và mapper.
- `PBMS.Infrastructure/DependencyInjection.cs` đăng ký concrete repository, `DbContext`, external service và cấu hình hạ tầng.
- `PBMS.API/Program.cs` chỉ chịu trách nhiệm compose và gọi các extension method của từng tầng:
  - `builder.Services.AddApplicationServices();`
  - `builder.Services.AddInfrastructureServices(builder.Configuration);`
- Quy tắc đơn giản:
  - interface/service của application chỉ đăng ký trong `PBMS.Application`.
  - implementation của infrastructure chỉ đăng ký trong `PBMS.Infrastructure`.
  - một service chỉ nên đăng ký một lần, ở đúng layer tương ứng.
- Team có thể để từng thành viên đăng ký service của module mình, nhưng cần review PR để tránh duplicate và sai layer.

---

## Cấu trúc thư mục

### PBMS.API

```
PBMS.API/
├── Controllers/
│   ├── AuthController.cs
│   ├── BuildingController.cs
│   ├── ZoneController.cs
│   ├── SlotController.cs
│   ├── VehicleController.cs
│   ├── ParkingSessionController.cs
│   ├── BookingController.cs
│   ├── MonthlyCardController.cs
│   ├── PaymentController.cs
│   ├── PricingPolicyController.cs
│   ├── IncidentController.cs
│   └── RevenueController.cs
├── Middlewares/
│   ├── ExceptionHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

### PBMS.Application

Mỗi domain có folder riêng, bên trong gồm `DTOs/`, `Interfaces/`, `Services/`:

```
PBMS.Application/
├── Common/
│   ├── Exception/
│   │   ├── AppException.cs          ← Base class cho lỗi ở tầng Application
│   │   ├── NotFoundException.cs     ← Throw khi tìm ID không thấy (dùng cho mọi entity) -> Middleware sẽ map ra lỗi 404
│   │   ├── ValidationException.cs   ← Throw khi dữ liệu request bị sai lệch -> Middleware sẽ map ra lỗi 400
│   │   └── ForbiddenException.cs    ← Throw khi user sai role -> Middleware sẽ map ra lỗi 403
│   ├── BaseResponse.cs
│   └── PagedResult.cs
├── Contracts/                       ← Repository interfaces (Infrastructure implement)
│   ├── IRepository.cs               ← Generic repository interface
│   ├── IUnitOfWork.cs
│   ├── IBookingRepository.cs
│   ├── IParkingSessionRepository.cs
│   ├── IMonthlyCardRepository.cs
│   ├── IPaymentRepository.cs
│   └── ...
├── Auth/
├── ParkingStructure/                ← Building, Floor, Zone, Slot
├── Vehicle/                         ← VehicleType, Vehicle, Driver account
├── ParkingSession/                  ← Check-in, Check-out, Session tracking
├── Booking/                         ← Tạo booking, confirm, cancel, no-show
├── MonthlyCard/                     ← Cấp thẻ, gia hạn, downgrade
├── Pricing/                         ← PricingPolicy, PricingWindow, FeeCalculation
├── Payment/                         ← Cash, Online payment
├── Incident/                        ← Xử lý ngoại lệ: mất mã, sai biển số...
└── Revenue/                         ← Thống kê doanh thu, tỷ lệ lấp đầy
```

Cấu trúc bên trong mỗi domain (ví dụ `Booking/`):

```
Booking/
├── DTOs/
│   ├── CreateBookingRequest.cs
│   ├── UpdateBookingRequest.cs
│   └── BookingDto.cs
├── Interfaces/
│   └── IBookingService.cs           ← Service interface (Controller inject)
└── Services/
    └── BookingService.cs
```

> **Phân biệt 2 loại Interface trong Application:**
> - `Contracts/I...Repository.cs` — giao tiếp xuống Infrastructure (DB)
> - `{Domain}/Interfaces/I...Service.cs` — giao tiếp lên API (Controller inject)

### PBMS.Domain

```
PBMS.Domain/
├── Entities/
│   ├── Building.cs
│   ├── Floor.cs
│   ├── Zone.cs
│   ├── ParkingSlot.cs
│   ├── Vehicle.cs
│   ├── VehicleType.cs
│   ├── Account.cs
│   ├── Role.cs
│   ├── ParkingSession.cs
│   ├── Booking.cs
│   ├── MonthlyCard.cs
│   ├── PricingPolicy.cs
│   ├── PricingWindow.cs
│   ├── Payment.cs
│   ├── Incident.cs
│   ├── IncidentType.cs
│   ├── Notification.cs
│   ├── RevenueStatistic.cs
│   └── RevenueStatisticPayment.cs
├── Enums/
│   ├── SlotStatus.cs               ← AVAILABLE, RESERVED, OCCUPIED, MAINTENANCE
│   ├── BookingStatus.cs            ← PENDING, CONFIRMED, CANCELLED, EXPIRED, COMPLETED
│   ├── CardStatus.cs               ← ACTIVE, EXPIRED, SUSPENDED
│   ├── SessionStatus.cs            ← ACTIVE, COMPLETED
│   ├── PaymentMethod.cs            ← CASH, ONLINE
│   └── PaymentStatus.cs            ← PENDING, SUCCESS, FAILED
└── Exceptions/                     ← Exception nghiệp vụ, throw từ Service, bắt ở Middleware
    ├── DomainException.cs          ← Base class cho tất cả exception bên dưới
    ├── SlotNotAvailableException.cs
    ├── BookingNotFoundException.cs
    ├── BookingExpiredException.cs
    ├── MonthlyCardExpiredException.cs
    ├── DuplicateLicensePlateException.cs
    └── InvalidCheckOutException.cs
```

> **Tại sao Exception nằm trong Domain?**
> Exception nghiệp vụ (slot đã có người đặt, booking hết hạn...) là kiến thức của domain, không phải của tầng API hay Infrastructure. Service throw exception, `ExceptionHandlingMiddleware` trong API bắt lại và map thành HTTP status code phù hợp (400, 404, 409...).

### PBMS.Infrastructure

```
PBMS.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   └── UnitOfWork.cs
├── Repositories/
│   ├── BaseRepository.cs
│   ├── BuildingRepository.cs
│   ├── ParkingSessionRepository.cs
│   ├── BookingRepository.cs
│   ├── MonthlyCardRepository.cs
│   ├── PaymentRepository.cs
│   └── ...
├── Configurations/             ← EF Core Fluent API config cho từng entity
│   ├── BuildingConfiguration.cs
│   ├── ParkingSessionConfiguration.cs
│   └── ...
├── Migrations/
└── ExternalServices/
    └── BankPaymentService.cs   ← Tích hợp thanh toán online qua ngân hàng
```

---

## Phân chia domain

Bảng dưới mô tả từng domain, file liên quan và lưu ý nghiệp vụ quan trọng:

| Domain | Folder trong Application | Lưu ý nghiệp vụ |
|---|---|---|
| **Auth** | `Auth/` | JWT, phân quyền Manager / Staff / Driver |
| **Parking Structure** | `ParkingStructure/` | Building → Floor → Zone → Slot; trạng thái slot theo `SlotStatus` |
| **Vehicle** | `Vehicle/` | Một Driver account có thể có nhiều xe; tạo account trước khi có xe |
| **Parking Session** | `ParkingSession/` | Check-in tạo session, check-out tính phí và đóng session |
| **Booking** | `Booking/` | Bắt buộc có biển số, deposit fee = giá block đầu tiên; tối thiểu 1h, tối đa 8h trước giờ đến |
| **Monthly Card** | `MonthlyCard/` | Xe máy: đảm bảo có chỗ, không gán slot cụ thể. Ô tô: bắt buộc gán `assigned_slot_id` |
| **Pricing** | `Pricing/` | Tách `PricingService` (CRUD policy) và `FeeCalculationService` (tính phí theo Time Window, Block Pricing, Window Cap, grace period) |
| **Payment** | `Payment/` | Hỗ trợ tiền mặt (làm tròn theo cash rounding rule) và online (giữ nguyên giá trị chính xác) |
| **Incident** | `Incident/` | Mất mã gửi xe, sai biển số, quá hạn, xe gửi sai khu vực |
| **Revenue** | `Revenue/` | Tổng hợp doanh thu, lượt xe, tỷ lệ lấp đầy theo Building/Zone/VehicleType |

### Lưu ý đặc biệt: FeeCalculationService

`FeeCalculationService` nên được tách riêng hoàn toàn và giao cho **một người** phụ trách vì logic phức tạp:

- Tính phí theo **Time Window** — khung giờ ngày/đêm
- Nếu session trải qua nhiều khung giờ → tách session, tính phí riêng từng khung
- **Window Cap** chỉ áp dụng trong từng khung giờ, không áp dụng toàn session
- **Grace period** — thời gian phát sinh ≤ grace period thì không tính thêm block
- **Cash rounding** — làm tròn tiền mặt; online giữ nguyên
- **Downgrade** — thẻ tháng hết hạn khi xe vẫn trong bãi → tính phí vãng lai từ thời điểm hết hạn

---

## Quy ước đặt tên

### File & Class

| Loại | Quy ước | Ví dụ |
|---|---|---|
| Entity | `PascalCase` | `ParkingSession.cs` |
| Interface | `I` + tên | `IParkingSessionService.cs` |
| Service | tên + `Service` | `ParkingSessionService.cs` |
| Repository | tên + `Repository` | `ParkingSessionRepository.cs` |
| Controller | tên + `Controller` | `ParkingSessionController.cs` |
| DTO (response) | tên + `Dto` | `BookingDto.cs` |
| DTO (request) | action + `Request` | `CreateBookingRequest.cs`, `CheckInRequest.cs` |
| Enum | `PascalCase` | `SlotStatus.cs` |
| Config (EF) | tên + `Configuration` | `BookingConfiguration.cs` |

### Endpoint (REST API)

```
GET    /api/bookings                  ← danh sách
GET    /api/bookings/{id}             ← chi tiết
POST   /api/bookings                  ← tạo mới
PUT    /api/bookings/{id}             ← cập nhật
DELETE /api/bookings/{id}             ← xoá / cancel

POST   /api/parking-sessions/check-in
POST   /api/parking-sessions/check-out
GET    /api/slots/{id}/status
```

Dùng `kebab-case` cho URL, `camelCase` cho JSON response.

### Namespace

```
PBMS.API.Controllers
PBMS.Application.Booking.Services
PBMS.Application.Booking.Interfaces
PBMS.Application.Booking.DTOs
PBMS.Application.Contracts           ← Repository interfaces
PBMS.Domain.Entities
PBMS.Domain.Enums
PBMS.Domain.Exceptions
PBMS.Infrastructure.Repositories
PBMS.Infrastructure.Data
```

---

## Hướng dẫn bắt đầu

### Yêu cầu

- .NET 8 SDK
- SQL Server (hoặc PostgreSQL — tuỳ config của team)
- Visual Studio 2022 hoặc Rider hoặc VS Code + C# extension

### Chạy project lần đầu

```bash
# 1. Clone repo
git clone <repo-url>
cd pbms-backend

# 2. Restore packages
dotnet restore

# 3. Cập nhật connection string trong appsettings.Development.json
# "ConnectionStrings": { "Default": "Server=...;Database=PBMS;..." }

# 4. Chạy migration
dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API

# 5. Chạy API
dotnet run --project src/PBMS.API
```

Swagger UI sẽ có tại: `https://localhost:{port}/swagger`

---

## Quy trình làm việc nhóm (Git workflow)

### Nhánh

```
main          ← code ổn định, đã review (không push thẳng)
develop       ← nhánh tích hợp chung, merge feature vào đây
feature/      ← nhánh tính năng của từng người
fix/          ← nhánh sửa bug
```

### Tạo nhánh mới

```bash
# Luôn tạo nhánh từ develop, không từ main
git checkout develop
git pull origin develop
git checkout -b feature/booking-service
```

### Tên nhánh

```
feature/parking-session-checkin
feature/fee-calculation-service
feature/monthly-card-management
fix/booking-deposit-calculation
```

### Commit message

```
feat: add check-in logic for walk-in vehicle
feat: implement fee calculation with window cap
fix: correct cash rounding for session checkout
refactor: extract time window splitting to helper method
chore: add EF config for PricingWindow entity
```

### Merge về develop

1. Push nhánh feature lên remote
2. Tạo **Pull Request** (PR) vào `develop`
3. Cần ít nhất **1 người review** trước khi merge
4. Không tự merge PR của mình

### Tránh conflict

Mỗi người làm việc trong folder domain của mình. Các file dùng chung cần cẩn thận:

| File dùng chung | Ai quản lý | Cách xử lý |
|---|---|---|
| `AppDbContext.cs` | Người phụ trách Infrastructure | Thông báo nhóm trước khi sửa; khi thêm `DbSet` mới thì assign task rõ ràng |
| `Program.cs` | Người phụ trách Infrastructure | Tương tự — thông báo trước |
| `IUnitOfWork.cs` | Người phụ trách Infrastructure | Ít thay đổi sau khi setup xong |
| Entity files trong `PBMS.Domain` | Từng người theo domain | Không sửa entity của người khác |

---

## Phân công domain

> Cập nhật bảng này khi team phân công xong.

| Domain | Người phụ trách | Nhánh chính |
|---|---|---|
| Infrastructure setup (DbContext, UoW, Migration) | | |
| Auth (JWT, phân quyền) | | |
| Parking Structure (Building, Floor, Zone, Slot) | | |
| Vehicle & Driver Account | | |
| Parking Session (Check-in / Check-out) | | |
| Booking | | |
| Monthly Card | | |
| **Pricing & Fee Calculation** | | |
| Payment | | |
| Incident & Notification | | |
| Revenue & Reporting | | |

---
