> 🔍 Audited at commit: 21a886a — 2026-06-21

# 🏗️ Architecture Review

Tài liệu này đánh giá chi tiết cấu trúc kiến trúc hệ thống của dự án PBMS API dưới vai trò của một **Senior .NET Architect**, chỉ ra các điểm mạnh, điểm yếu, các điểm cải tiến đã thực hiện và danh sách khuyến nghị tái cấu trúc (Refactoring).

---

## 1. Clean Architecture Evaluation (Đánh giá Clean Architecture)

Hệ thống được tổ chức phân rã dự án theo cấu trúc Clean Architecture chuẩn với 4 layers chính. 
* **Mức độ tuân thủ:** **Tốt (90%)**.
* **Đặc điểm tích cực:** Lớp `Domain` và `Application` hoàn toàn sạch, không bị rò rỉ (leak) công nghệ EF Core hoặc các dependencies từ lớp ngoài vào.
* **Vấn đề phát hiện:** Lớp `PBMS.API` đang tham chiếu trực tiếp đến `PBMS.Infrastructure` để phục vụ đăng ký Dependency Injection ở [Program.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L43). Điều này phá vỡ tính cô lập tuyệt đối của lớp API đối với các chi tiết cài đặt ở Infrastructure.

---

## 2. SOLID Evaluation (Đánh giá SOLID)

* **Single Responsibility Principle (SRP - Đơn nhiệm):**
  * Các Controllers chỉ đảm nhận nhiệm vụ định tuyến và trả về HTTP Responses.
  * **Cải tiến:** Giá vé đăng ký tháng và giá phạt sự cố đã được bóc tách hoàn toàn ra khỏi cấu trúc tĩnh thành các cấu hình động độc lập (`SubscriptionPriceConfig`, `PenaltyConfig`) và tra cứu thông qua `FeeCalculatorService`.
  * *Hạn chế còn lại:* Lớp `BookingService.cs` vẫn đang gánh vác khá nhiều logic nghiệp vụ (kiểm tra sức chứa, tạo đơn, dọn dẹp trễ hẹn). Cần phân nhỏ tiếp ra các Domain Service chuyên biệt.
* **Open/Closed Principle (OCP - Đóng/Mở):**
  * Việc tách biệt các thực thể cấu hình giá giúp dễ dàng mở rộng các loại hình giá mới (ví dụ: giá phạt tùy biến, giá tháng ưu đãi) mà không cần can thiệp hay sửa đổi cấu trúc của các thực thể chính như `MonthlySubscription` hay `IncidentType`.
* **Liskov Substitution Principle (LSP):**
  * Triển khai tốt. Các repository cụ thể (như `BookingRepository`) kế thừa từ `BaseRepository` và bổ sung các hàm đặc thù mà không phá vỡ logic lớp cha.
* **Interface Segregation Principle (ISP):**
  * Giao diện repository và dịch vụ được chia tách rõ ràng theo phân hệ (như `IBookingService`, `ICardService` thay vì một Interface khổng lồ).
* **Dependency Inversion Principle (DIP):**
  * Áp dụng tuyệt đối thông qua việc Controllers và Services giao tiếp với nhau bằng Interfaces thay vì Concrete Classes.

---

## 3. Review Chi Tiết Từng Layer

### 🛡️ Tầng Domain Layer
* **Ưu điểm:** Các thực thể (Entities) kế thừa từ `BaseEntity` sạch sẽ, giải quyết tốt các thuộc tính audit (`CreatedAt`, `CreatedBy`,...) và Soft Delete (`IsDeleted`).
* **Hạn chế:** Logic nghiệp vụ (Domain Logic) còn ít (Anemic Domain Model). Hầu hết các Entity chỉ chứa thuộc tính dữ liệu. Cần chuyển dịch dần các logic tính toán (ví dụ: chuyển trạng thái booking sang checked-in) về viết trực tiếp bên trong các Domain Entities.

### ⚙️ Tầng Application Layer
* **Ưu điểm:** Phân loại rõ ràng thư mục Module. DTOs được tách biệt chi tiết giữa Request và Response.
* **Hạn chế:** Đăng ký AutoMapper trong [DependencyInjection.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/DependencyInjection.cs#L53) còn để trống cấu hình, dẫn đến việc map dữ liệu trong code Service đôi khi vẫn phải làm thủ công.

### 🗄️ Tầng Infrastructure Layer
* **Ưu điểm:** Triển khai Repository Pattern và Unit of Work thống nhất. Các file Migration được quản lý tốt.
* **Hạn chế:** VNPay Gateway (`VNPayGateway.cs`) đang cấu hình cứng một số thông số trực tiếp trong code thay vì đọc động hoàn toàn từ biến môi trường.

### 🔌 Tầng API Layer
* **Ưu điểm:** Xử lý lỗi tập trung thông qua `ExceptionHandlingMiddleware.cs` giúp che giấu Stack Trace thật khi có crash hệ thống ở môi trường Production.
* **Hạn chế:** API Controller thiếu phân quyền (Authorization) đồng nhất. Một số endpoint nhạy cảm (như thêm/xóa chính sách giá, dọn dẹp hệ thống) chưa được gán thẻ kiểm tra Role hợp lệ (`[Authorize(Roles = "Admin,Manager")]`).

---

## 4. Đánh Giá Khác

### A. Testing Readiness (Độ sẵn sàng kiểm thử)
* Dự án đã có sẵn project unit tests `PBMS.UnitTests` và một số test mẫu. Nhờ vá lỗi luồng và cấu trúc lỏng lẻo (Loose coupling), việc viết Mock dịch vụ để tăng diện bao phủ (test coverage) cho `FeeCalculationServiceTests`, `BookingServiceTests` và `ParkingSessionServiceTests` đã được triển khai hiệu quả (số test tăng từ 80 lên 83 và đạt 100% tỷ lệ đỗ).

### B. Scalability & Maintainability (Khả năng mở rộng & bảo trì)
* **Maintainability:** Rất cao nhờ sự tách biệt rõ ràng của Clean Architecture. Sửa đổi database không ảnh hưởng tới logic API.
* **Scalability:** Tốt cho quy mô vừa và lớn. Tuy nhiên, nếu lượng truy cập Check-In/Check-Out đồng thời quá cao, cơ chế khóa luồng `lock (_sync)` trong lớp In-Memory và các truy vấn DB trực tiếp không qua cache (Redis) có thể gây ra hiện tượng nghẽn cổ chai (bottleneck).

---

## 5. Danh Sách Khuyến Nghị Tái Cấu Trúc (Refactoring Recommendations)

Dưới đây phân loại khuyến nghị theo trạng thái sau đợt cập nhật:

### 🔍 Khuyến Nghị Còn Lại (Remaining Recommendations)

| Khuyến nghị | Layer | Ảnh hưởng | Độ ưu tiên | Dẫn chứng chi tiết |
| :--- | :--- | :--- | :--- | :--- |
| Chuyển logic tự động `EnsureDeleted()` ra ngoài luồng chạy API chính | API | Tránh nguy cơ mất sạch data Production | **P0** (Nghiêm trọng) | [Program.cs: L97](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L97) gọi lệnh xóa DB tự động nếu chạy debug. |
| Tách biệt cấu hình DI khởi động của lớp API và Infrastructure | API / Infrastructure | Tăng tính cô lập dự án | **P1** | [Program.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L43) tham chiếu trực tiếp lớp hiện thực của Infrastructure. |
| Bổ sung phân quyền `[Authorize(Roles = ...)]` cho các API quản trị chính sách | API | Bảo mật tài nguyên hệ thống | **P1** | `PricingPoliciesController` chưa có cấu hình phân quyền chặt chẽ trên từng Endpoint nhạy cảm. |
| Tổ chức Mapping Profile cho AutoMapper | Application | Rút gọn code map thủ công trong Service | **P2** | [DependencyInjection.cs: L53](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/DependencyInjection.cs#L53). |
| Chuyển dịch Anemic Domain Model sang Rich Domain Model | Domain | Tăng tính hướng đối tượng, giảm phình to Service | **P2** | Lớp `Booking` ở Domain chỉ chứa các thuộc tính dữ liệu. |

### ✅ Khuyến Nghị Đã Giải Quyết (Resolved Recommendations)

- **[Resolved - P1] Tách biệt logic cấu hình giá vé tháng và giá phạt sự cố:** Đã bóc tách hoàn toàn logic giá cứng từ `IncidentType` và `MonthlySubscription` ra các thực thể cấu hình riêng biệt kế thừa `ISoftDeletable` và kết nối thông qua `FeeCalculatorService`.
- **[Resolved - P0] Khắc phục lỗi tính phí gửi xe đêm sáng sớm:** Đảm bảo thuật toán phân đoạn thời gian xử lý chính xác và ổn định tuyệt đối (bao phủ bởi Unit Test tự động).
- **[Resolved - P1] Quản lý vòng đời Booking chặt chẽ:** Tự động giải phóng capacity Booking bằng cách chuyển sang `CheckedIn` tại thời điểm xe vào bãi, và tự động dọn dẹp các Booking Confirmed quá hạn thành `NoShow`.
