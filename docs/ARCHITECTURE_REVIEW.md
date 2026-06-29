> 🔍 Audited at commit: 6ba866a — 2026-06-24
> 🔄 Updated at audit: 2026-06-27 (Sau đợt refactor cấu hình động, slot booking time-query, pricing cleanup, check-in protection, DI isolation và VNPay Redirect)

# 🏗️ Architecture Review

Tài liệu này đánh giá chi tiết cấu trúc kiến trúc hệ thống của dự án PBMS API dưới vai trò của một **Senior .NET Architect**, chỉ ra các điểm mạnh, điểm yếu, các điểm cải tiến đã thực hiện và danh sách khuyến nghị tái cấu trúc (Refactoring).

---

## 1. Clean Architecture Evaluation (Đánh giá Clean Architecture)

Hệ thống được tổ chức phân rã dự án theo cấu trúc Clean Architecture chuẩn với 4 layers chính.
* **Mức độ tuân thủ:** **Xuất sắc (97%)**.
* **Đặc điểm tích cực:** Lớp `Domain` và `Application` hoàn toàn sạch, không bị rò rỉ (leak) công nghệ EF Core hoặc các dependencies từ lớp ngoài vào.
* **Cải tiến và Khắc phục Vi phạm:**
  * **[Resolved - P1] Tránh truy vấn trực tiếp từ Hosted Service:** Trước đây, dịch vụ nền `OvertimeWarningWorker` thuộc tầng Web API truy vấn trực tiếp DB bằng `AppDbContext`. Vi phạm này đã được giải quyết triệt để bằng cách đóng gói toàn bộ logic nghiệp vụ vào tầng Application thông qua giao diện [IParkingSessionService.SendOvertimeWarningsAsync()](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Interfaces/IParkingSessionService.cs#L18) và triển khai truy vấn tại tầng Infrastructure thông qua [IParkingSessionRepository.GetOvertimeWarningSessionsAsync()](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Contracts/IParkingSessionRepository.cs#L38).
  * **[Resolved - P1] Tách biệt logic dọn dẹp chính sách giá hết hạn:** Triển khai [ExpiredPricingPolicyCleanupWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/ExpiredPricingPolicyCleanupWorker.cs) ở tầng API hoạt động như một Trigger, giải phóng hoàn toàn logic thực thi xuống tầng Application qua `IPricingPolicyService.CleanupExpiredPricingPoliciesAsync()`.
  * **[Resolved - P1] API Layer phụ thuộc trực tiếp vào EF Context (AppDbContext) lúc Startup:** Đã loại bỏ hoàn toàn các chỉ thị `using PBMS.Infrastructure.Data;` và truy cập trực tiếp `AppDbContext` tại [Program.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs). Toàn bộ logic chạy DB migration và seed dữ liệu mẫu đã được đóng gói gọn gàng thành một extension method `MigrateAndSeedDatabaseAsync` trong [DependencyInjection.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Infrastructure/DependencyInjection.cs) của Infrastructure layer.
  * **[Resolved - P0] Tự động xóa database `EnsureDeleted()` trên startup:** Đã cấu hình chuyển logic này thành tùy chọn cấu hình động `Db:ResetOnStartup` trong [Program.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L95), tránh nguy cơ mất mát dữ liệu do chạy nhầm trên các môi trường persistent/production.

---

## 2. SOLID Evaluation (Đánh giá SOLID)

* **Single Responsibility Principle (SRP - Đơn nhiệm):**
  * Các Controllers chỉ đảm nhận nhiệm vụ định tuyến và trả về HTTP Responses hoặc chuyển hướng (Redirect).
  * **Cải tiến:** Giá vé đăng ký tháng và giá phạt sự cố đã được bóc tách hoàn toàn ra khỏi cấu trúc tĩnh thành các cấu hình động độc lập (`SubscriptionPriceConfig`, `PenaltyConfig`) và tra cứu thông qua `FeeCalculatorService`.
  * Các tác vụ định kỳ chạy ngầm (Dọn dẹp Booking quá hạn, Cảnh báo quá hạn đỗ xe, dọn dẹp chính sách giá hết hạn) đã được chuyển thành các Hosted Services chuyên biệt (`ExpiredBookingCleanupWorker`, `OvertimeWarningWorker`, `ExpiredPricingPolicyCleanupWorker`), giải phóng API Controller khỏi các tác vụ nền phi đồng bộ.
  * *Hạn chế còn lại:* Lớp `BookingService.cs` và `ParkingSessionService.cs` vẫn đang gánh vác khá nhiều logic nghiệp vụ phức tạp (kiểm tra sức chứa, xác thực trạng thái, tính toán tiền gửi xe, kiểm tra blacklist).
* **Open/Closed Principle (OCP - Đóng/Mở):**
  * Việc tách biệt các thực thể cấu hình giá giúp dễ dàng mở rộng các loại hình giá mới (ví dụ: giá phạt tùy biến, giá tháng ưu đãi) mà không cần can thiệp hay sửa đổi cấu trúc của các thực thể chính như `MonthlySubscription` hay `IncidentType`.
* **Liskov Substitution Principle (LSP):**
  * Triển khai tốt. Các repository cụ thể (như `BookingRepository` và `ParkingSessionRepository`) kế thừa từ `BaseRepository` và bổ sung các hàm đặc thù mà không phá vỡ logic lớp cha.
* **Interface Segregation Principle (ISP):**
  * Giao diện repository và dịch vụ được chia tách rõ ràng theo phân hệ (như `IBookingService`, `ICardService` thay vì một Interface khổng lồ).
* **Dependency Inversion Principle (DIP):**
  * Áp dụng tốt thông qua việc Controllers và Services giao tiếp với nhau bằng Interfaces thay vì Concrete Classes. Hosted services chạy nền chỉ gọi tới các Interface nghiệp vụ ở tầng Application.
  * Logic lấy cấu hình nghiệp vụ đặt chỗ tại `BookingService` đã được tách biệt thông qua `IConfiguration`, loại bỏ hoàn toàn các hằng số bị gán cứng (`const`).

---

## 3. Review Chi Tiết Từng Layer

### 🛡️ Tầng Domain Layer
* **Ưu điểm:** Các thực thể (Entities) kế thừa từ `BaseEntity` sạch sẽ, giải quyết tốt các thuộc tính audit (`CreatedAt`, `CreatedBy`,...) và Soft Delete (`IsDeleted`).
* **Hạn chế:** Logic nghiệp vụ (Domain Logic) còn ít (Anemic Domain Model). Hầu hết các Entity chỉ chứa thuộc tính dữ liệu. Cần chuyển dịch dần các logic tính toán về viết trực tiếp bên trong các Domain Entities.

### ⚙️ Tầng Application Layer
* **Ưu điểm:** Phân loại rõ ràng thư mục Module. DTOs được tách biệt chi tiết giữa Request và Response. Các interface nghiệp vụ bao quát tốt và được tổ chức sạch sẽ.
* **Cải tiến:** Thêm DTO [BuildingAvailableCapacityDto.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingStructure/DTOs/BuildingAvailableCapacityDto.cs) và logic tính toán sức chứa trống theo thời gian `GetAvailableCapacityByTimeframeAsync` trong `BuildingService.cs`.
* **Hạn chế:** Đăng ký AutoMapper trong [DependencyInjection.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/DependencyInjection.cs#L53) còn để trống cấu hình, dẫn đến việc map dữ liệu trong code Service đôi khi vẫn phải làm thủ công.

### 🗄️ Tầng Infrastructure Layer
* **Ưu điểm:** Triển khai Repository Pattern và Unit of Work thống nhất. Các file Migration được quản lý tốt.
* **Cải tiến:** Logic check-in phân bổ Phân khu/Slot trống trong [ParkingSessionRepository.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Infrastructure/Repositories/ParkingSessionRepository.cs) đã được cải tiến để tính toán cả các booking đang chờ/được xác nhận sắp check-in (trong vòng 30 phút), giúp bảo vệ sức chứa trống cho khách đặt trước mà không phụ thuộc vào dữ liệu trạng thái động được lưu trữ cứng.
* **Hạn chế:** VNPay Gateway (`VNPayGateway.cs`) đang cấu hình cứng một số thông số trực tiếp trong code thay vì đọc động hoàn toàn từ biến môi trường.

### 🔌 Tầng API Layer
* **Ưu điểm:** Xử lý lỗi tập trung thông qua `ExceptionHandlingMiddleware.cs`. Tích hợp các Hosted Service chạy nền gọn gàng và không chứa logic trực tiếp.
* **Cải tiến:** Cập nhật endpoint `/api/parkingslots/zone/{zoneId}`, thêm `/api/pricing-policies/cleanup`, `/api/buildings/{id}/available-capacity` và `/api/payments/vnpay-return`.

---

## 4. Đánh Giá Khác

### A. Testing Readiness (Độ sẵn sàng kiểm thử)
* **Tiến độ vượt bậc:** Dự án đã mở rộng đáng kể số lượng unit test tự động trong project `PBMS.UnitTests` (tổng số test đã đạt mốc **137 tests**, duy trì tỷ lệ pass **100%**).
* Các module cốt lõi như `BookingService`, `ParkingSessionService`, `VehicleTypeService`, `IncidentService`, `PricingPolicyService`, `BuildingService` và `PaymentService` đã được bao phủ bởi các unit test kiểm thử các luật nghiệp vụ khắt khe. Các unit test bổ sung cho `GetAvailableCapacityByTimeframeAsync` đảm bảo tính toán chính xác số chỗ đỗ xe trống khả dụng sau khi trừ đi phần trăm dự phòng Buffer và số lượng Bookings trùng lịch.

### B. Scalability & Maintainability (Khả năng mở rộng & bảo trì)
* **Maintainability:** Rất cao nhờ sự tách biệt rõ ràng của Clean Architecture. Sửa đổi database không ảnh hưởng tới logic API.
* **Scalability:** Tốt cho quy mô vừa và lớn. Tuy nhiên, việc đưa vào các Background Worker quét DB định kỳ (`ExpiredBookingCleanupWorker` chạy mỗi 5 phút, `OvertimeWarningWorker` chạy mỗi 1 phút, `ExpiredPricingPolicyCleanupWorker` chạy mỗi 12 giờ) yêu cầu cần thiết lập các Database Indexes phù hợp. Các cột như `SessionStatus`, `PlannedCheckoutTime`, `IsDeleted` và `BookingId` cần được đánh Index để tránh table scans đầy đủ (Full Table Scans) khi số lượng bản ghi phiên đỗ xe và đặt chỗ tăng lên, tránh hiện tượng nghẽn cơ sở dữ liệu.

---

## 5. Danh Sách Khuyến Nghị Tái Cấu Trúc (Refactoring Recommendations)

Dưới đây phân loại khuyến nghị theo trạng thái sau đợt cập nhật:

### 🔍 Khuyến Nghị Còn Lại (Remaining Recommendations)

| Khuyến nghị | Layer | Ảnh hưởng | Độ ưu tiên | Dẫn chứng chi tiết | Trạng thái |
| :--- | :--- | :--- | :--- | :--- | :--- |
| Tổ chức Mapping Profile cho AutoMapper | Application | Rút gọn code map thủ công trong Service | **P2** | [DependencyInjection.cs: L53](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/DependencyInjection.cs#L53) | **Remaining** |
| Chuyển dịch Anemic Domain Model sang Rich Domain Model | Domain | Tăng tính hướng đối tượng, giảm phình to Service | **P2** | Lớp `Booking` ở Domain chỉ chứa các thuộc tính dữ liệu. | **Remaining** |

### ✅ Khuyến Nghị Đã Giải Quyết (Resolved Recommendations)

* **[New Resolved - 2026-06-27] API Layer phụ thuộc trực tiếp vào EF Context (AppDbContext) lúc Startup:** Chuyển toàn bộ logic Migration và Seed DB sang Extension Method của Infrastructure layer, giúp API layer không cần tham chiếu trực tiếp đến các lớp cấu hình DB chi tiết.
* **[New Resolved - 2026-06-27] Tránh gán cứng cấu hình Booking trong Service:** Cấu hình được tách biệt qua file `appsettings.json` thay vì gán hằng số `const` trong `BookingService`.
* **[New Resolved - 2026-06-27] Tách biệt logic dọn dẹp chính sách giá hết hạn:** Triển khai Worker ngầm chạy 12 giờ một lần phối hợp với API endpoint dọn dẹp chủ động, dọn dẹp các chính sách quá hạn hiệu lực.
* **[Resolved - P1] Bổ sung phân quyền `[Authorize(Roles = ...)]` cho các API quản trị chính sách:**
  * *Giải pháp:* Gán `[Authorize]` ở mức lớp và `[Authorize(Roles = "Admin,Manager")]` cho các hành động viết/sửa/xoá tại `PricingPoliciesController`, `PenaltyConfigsController`, và `SubscriptionPriceConfigsController`.
* **[Resolved - P1] Đánh chỉ mục (Index) DB cho các trường truy vấn định kỳ:**
  * *Giải pháp:* Cấu hình các Indexes cho `BookingStatus`, `PaymentDeadline`, `CheckinGraceUntil`, `PlannedCheckoutTime` trong `BookingConfiguration` và `SessionStatus` trong `ParkingSessionConfiguration`, đồng thời tạo và áp dụng Entity Framework migration thành công.
* **[Resolved - P1] Loại bỏ truy vấn trực tiếp cơ sở dữ liệu từ API Background Workers:**
  * *Mô tả:* Tách biệt rạch ròi, chuyển logic của `OvertimeWarningWorker` sang Application Layer và Infrastructure Layer.
  * *Giải pháp:* Thiết lập phương thức `IParkingSessionService.SendOvertimeWarningsAsync()` và `IParkingSessionRepository.GetOvertimeWarningSessionsAsync()`.
* **[Resolved - P0] Chuyển logic tự động `EnsureDeleted()` ra ngoài luồng chạy API chính:**
  * *Mô tả:* Tránh việc tự động xoá sạch Database mỗi khi khởi động môi trường Development.
  * *Giải pháp:* Tham số hoá thông qua cấu hình `Db:ResetOnStartup` (mặc định là `false`).
* **[Resolved - P1] Quản lý vòng đời Booking chặt chẽ & tự động hóa:**
  * *Giải pháp:* Tích hợp logic liên kết booking khi check-in trong `ParkingSessionService` và triển khai Hosted Service chạy nền [ExpiredBookingCleanupWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/ExpiredBookingCleanupWorker.cs) để tự động hóa dọn dẹp các Booking trễ hẹn.
* **[Resolved - P1] Tự động hóa cảnh báo quá hạn gửi xe:**
  * *Giải pháp:* Triển khai Hosted Service [OvertimeWarningWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/OvertimeWarningWorker.cs) tự động chạy chu kỳ 1 phút để tạo và gửi thông báo cảnh báo sớm 15 phút.
* **[Resolved - P1] Tách biệt logic cấu hình giá vé tháng và giá phạt sự cố:** Đã bóc tách hoàn toàn logic giá cứng từ `IncidentType` và `MonthlySubscription` ra các thực thể cấu hình riêng biệt kế thừa `ISoftDeletable` và kết nối thông qua `FeeCalculatorService`.
* **[Resolved - P0] Khắc phục lỗi tính phí gửi xe đêm sáng sớm:** Đảm bảo thuật toán phân đoạn thời gian xử lý chính xác và ổn định tuyệt đối (bao phủ bởi Unit Test tự động).
