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
  - [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
  - [Cấu hình chạy Local (Database cá nhân)](#các-bước-cấu-hình-chạy-local-database-cá-nhân)
  - [Quy trình sửa đổi Entity & tạo Migration mới](#quy-trình-dành-cho-lập-trình-viên-khi-sửa-đổi-entity--tạo-migration-mới)
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

### Yêu cầu hệ thống

- **.NET 10 SDK** (phiên bản phù hợp với dự án)
- **PostgreSQL** (chạy trực tiếp trên máy qua pgAdmin hoặc Docker container)
- **Visual Studio 2022**, **JetBrains Rider**, hoặc **VS Code** (cần cài đặt extension C# Dev Kit)
- **EF Core CLI tool**: Hỗ trợ chạy lệnh tạo migration. Cài đặt bằng lệnh:
  ```bash
  dotnet tool install --global dotnet-ef
  # Hoặc nếu đã cài trước đó thì cập nhật bản mới nhất:
  dotnet tool update --global dotnet-ef
  ```

### Các bước cấu hình chạy Local (Database cá nhân)

Để tránh làm rác cơ sở dữ liệu chung trên Supabase trong quá trình phát triển và kiểm thử, các thành viên **bắt buộc** phải sử dụng Database PostgreSQL cá nhân ở local:

#### Bước 1: Chuẩn bị DB PostgreSQL local
* Hãy chắc chắn bạn đã khởi động PostgreSQL trên máy và biết tài khoản/mật khẩu kết nối.
* Bạn không cần tạo trước cơ sở dữ liệu (Database), EF Core sẽ tự động tạo cho bạn ở bước sau.

#### Bước 2: Cấu hình Connection String cục bộ
* Mở file [appsettings.Development.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/appsettings.Development.json) trong dự án `PBMS.API` (file này đã được đưa vào `.gitignore` để tránh bị đẩy đè lên Git).
* Thêm chuỗi kết nối local của bạn như sau:
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Host=localhost;Database=pbms_local;Username=postgres;Password=MAT_KHAU_CUA_BAN;SSL Mode=Prefer;Trust Server Certificate=true"
    },
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    }
  }
  ```
  *(Thay `MAT_KHAU_CUA_BAN` bằng mật khẩu PostgreSQL máy bạn).*

#### Bước 3: Đồng bộ cấu trúc bảng và chạy ứng dụng
Dự án đã được tích hợp cơ chế tự động chạy Migration khi khởi động ở môi trường Development. Bạn chỉ cần chạy ứng dụng:
```bash
# Di chuyển đến thư mục API và chạy
dotnet run --project src/PBMS.API
```
* **Kết quả:** EF Core sẽ tự động tạo database `pbms_local` trên máy bạn, chạy toàn bộ các file migration hiện có và hiển thị thông báo: `--> Database migration completed successfully.`
* Swagger UI sẽ tự động mở tại địa chỉ: `http://localhost:{port}/swagger` (hoặc thông tin hiển thị trên console).

---

### Quy trình dành cho lập trình viên khi sửa đổi Entity & tạo Migration mới

Khi bạn nhận nhiệm vụ phát triển một thực thể (Entity) mới hoặc thay đổi cấu trúc bảng, hãy tuân thủ quy trình sau để tránh gây xung đột (conflict) mã nguồn DB:

#### 1. Viết/Sửa Entity & Khai báo
* Thêm thực thể mới vào thư mục `src/PBMS.Domain/Entities`.
* Khai báo `DbSet<T>` tương ứng vào [AppDbContext.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Infrastructure/Data/AppDbContext.cs).
* *(Tùy chọn)* Cấu hình chi tiết kiểu dữ liệu, ràng buộc khóa ngoại bằng Fluent API trong thư mục `src/PBMS.Infrastructure/Configurations`.

#### 2. Tạo file Migration (ở Local)
Chạy lệnh sau tại thư mục gốc của dự án để EF Core ghi nhận thay đổi cấu trúc:
```bash
dotnet ef migrations add Add<TenTinhNang> --project src/PBMS.Infrastructure --startup-project src/PBMS.API
```
*Lưu ý: Thay `Add<TenTinhNang>` bằng mô tả ngắn gọn (ví dụ: `AddParkingSession`, `UpdateAccountStatus`).*

#### 3. Chạy thử nghiệm local
* Khởi chạy dự án API (`dotnet run`). Hệ thống sẽ tự động cập nhật database local của bạn.
* Thực hiện gọi thử các API để kiểm tra hoạt động của tính năng.

#### 4. Quy tắc tránh xung đột (Conflict) trước khi merge PR
Trước khi tạo Pull Request để merge nhánh tính năng của bạn vào `develop`, hãy đảm bảo nhánh của bạn đồng bộ lịch sử migration:
1. Chuyển về nhánh `develop` và pull code mới nhất: `git checkout develop && git pull origin develop`.
2. Quay lại nhánh của bạn: `git checkout feature/<ten-nhanh>`.
3. Trộn `develop` vào nhánh của bạn: `git merge develop`.
4. **Nếu xảy ra conflict ở file `AppDbContextModelSnapshot.cs`:**
   * Cách giải quyết an toàn nhất: Xóa file Migration bạn vừa tạo ở local đi: `dotnet ef migrations remove --project src/PBMS.Infrastructure --startup-project src/PBMS.API`.
   * Thực hiện `git merge develop` lại để đồng bộ snapshot mới nhất của nhóm.
   * Chạy lại lệnh tạo migration (ở bước 2). Lúc này file migration mới của bạn sẽ được đặt nối tiếp sau toàn bộ các migration của các thành viên khác.
5. Kiểm tra chạy thử lại lần cuối, sau đó push và tạo PR.


### Cấu hình Google OAuth2 (Đăng nhập bằng Google)

Tính năng đăng nhập bằng Google OAuth2 đã được tích hợp sẵn ở Backend. Để chạy thử hoặc triển khai, bạn cần cấu hình:

1. **Chạy cục bộ (Local)**:
   Mở file `src/PBMS.API/appsettings.json` và cấu hình Google Client ID của bạn vào:
   ```json
   "Google": {
     "ClientId": "768808098768-vop4tnm5u22h8stb6464bqtogse2rqvm.apps.googleusercontent.com"
   }
   ```

2. **Chạy trong môi trường Docker**:
   Trong file `docker-compose.yml`, biến môi trường `Google__ClientId` đã được cấu hình sẵn đè lên cấu hình `appsettings.json`:
   ```yaml
   environment:
     - Google__ClientId=768808098768-vop4tnm5u22h8stb6464bqtogse2rqvm.apps.googleusercontent.com
   ```

3. **Cơ chế hoạt động**:
   * **Endpoint tiếp nhận**: `POST /api/auth/google` (Nhận mã `IdToken` từ Client gửi lên).
   * **Tự động đăng ký**: Nếu tài khoản Gmail chưa tồn tại trong hệ thống, Backend sẽ tự động đăng ký mới với vai trò là **Driver** (RoleId = 3) và gán trạng thái `Active`.
   * **Tự động liên kết**: Nếu tài khoản Gmail đã đăng ký bằng email/mật khẩu thông thường từ trước, Backend tự động liên kết (link) hai tài khoản làm một mà không gây trùng lặp dữ liệu.
   * **Kiểm thử local (Swagger)**: Để kiểm thử tính năng này trên Swagger UI khi chưa có database thật, hệ thống đã cài đặt một Database RAM ảo tĩnh (`AccountRepository.cs`). Khi tắt/bật lại server, dữ liệu đăng ký Driver ảo mới sẽ bị reset.

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
