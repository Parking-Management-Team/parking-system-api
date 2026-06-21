# 🏗️ Architecture Review

Tài liệu này đánh giá chi tiết cấu trúc kiến trúc hệ thống của dự án PBMS API dưới vai trò của một **Senior .NET Architect**, chỉ ra các điểm mạnh, điểm yếu và các khuyến nghị tái cấu trúc (Refactoring).

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
  * Tuy nhiên, lớp `BookingService.cs` đang gánh vác quá nhiều logic nghiệp vụ bao gồm: tính giá cọc, kiểm tra sức chứa tòa nhà, tạo đơn, kiểm tra thời hạn và giải phóng bộ nhớ. Cần phân nhỏ ra các Domain Service chuyên biệt.
* **Open/Closed Principle (OCP - Đóng/Mở):**
  * Cấu hình DbContext linh hoạt qua Fluent API giúp dễ dàng mở rộng Entity mới mà không ảnh hưởng cấu trúc cũ.
* **Liskov Substitution Principle (LSP):**
  * Triển khai tốt. Các repository cụ thể (như `BookingRepository`) kế thừa thành công từ `BaseRepository` và bổ sung các hàm đặc thù mà không phá vỡ logic lớp cha.
* **Interface Segregation Principle (ISP):**
  * Giao diện repository và dịch vụ được chia tách rõ ràng theo phân hệ (như `IBookingService`, `ICardService` thay vì một Interface khổng lồ).
* **Dependency Inversion Principle (DIP):**
  * Áp dụng tuyệt đối thông qua việc Controllers và Services giao tiếp với nhau bằng Interfaces thay vì Concrete Classes.

---

## 3. Review Chi Tiết Từng Layer

### 🛡️ Tầng Domain Layer
* **Ưu điểm:** Các thực thể (Entities) kế thừa từ `BaseEntity` rất sạch sẽ, giải quyết tốt các thuộc tính audit (`CreatedAt`, `CreatedBy`,...) và Soft Delete (`IsDeleted`).
* **Hạn chế:** Logic nghiệp vụ (Domain Logic) còn quá ít. Hầu hết các Entity chỉ chứa thuộc tính dữ liệu (Anemic Domain Model). Cần chuyển dịch các logic tính toán (ví dụ: chuyển trạng thái booking, tính tiền phạt quá hạn) từ Application Services về viết trực tiếp bên trong các Domain Entities.

### ⚙️ Tầng Application Layer
* **Ưu điểm:** Phân loại rõ ràng thư mục Module (Auth, Booking, ParkingSession, Payment, Revenue, ...). DTOs được tách biệt chi tiết giữa Request và Response.
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
* Dự án đã có sẵn project unit tests `PBMS.UnitTests` và một số test mẫu cho `ParkingSessionService` và `PricingPolicy`. Nhờ kiến trúc lỏng lẻo (Loose coupling), việc viết Mock dịch vụ (sử dụng Moq hoặc NSubstitute) để phủ sóng 100% test coverage cho `BookingService` hay `PaymentService` là hoàn toàn khả thi và dễ dàng thực hiện.

### B. Scalability & Maintainability (Khả năng mở rộng & bảo trì)
* **Maintainability:** Rất cao nhờ sự tách biệt rõ ràng của Clean Architecture. Sửa đổi database không ảnh hưởng tới logic API.
* **Scalability:** Tốt cho quy mô vừa và lớn. Tuy nhiên, nếu lượng truy cập Check-In/Check-Out đồng thời quá cao (ví dụ: bãi xe tòa nhà lớn giờ cao điểm), cơ chế khóa luồng `lock (_sync)` trong lớp In-Memory và các truy vấn DB trực tiếp không qua cache (Redis) có thể gây ra hiện tượng nghẽn cổ chai (bottleneck).

---

## 5. Danh Sách Khuyến Nghị Tái Cấu Trúc (Refactoring Recommendations)

| Khuyến nghị | Layer | Ảnh hưởng | Độ ưu tiên | Dẫn chứng chi tiết |
| :--- | :--- | :--- | :--- | :--- |
| Tách biệt cấu hình DI khởi động của lớp API và Infrastructure | API / Infrastructure | Tăng tính cô lập dự án | **P1** | [Program.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L43) tham chiếu trực tiếp lớp hiện thực của Infrastructure. |
| Chuyển logic tự động `EnsureDeleted()` ra ngoài luồng chạy API chính | API | Tránh nguy cơ mất sạch data Production | **P0** (Nghiêm trọng) | [Program.cs: L97](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Program.cs#L97) gọi lệnh xóa DB tự động nếu chạy debug. |
| Bổ sung phân quyền `[Authorize(Roles = ...)]` cho các API quản trị chính sách | API | Bảo mật tài nguyên hệ thống | **P1** | `PricingPoliciesController` chưa có cấu hình phân quyền chặt chẽ trên từng Endpoint nhạy cảm. |
| Tổ chức Mapping Profile cho AutoMapper | Application | Rút gọn code map thủ công trong Service | **P2** | [DependencyInjection.cs: L53](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/DependencyInjection.cs#L53). |
| Chuyển dịch Anemic Domain Model sang Rich Domain Model | Domain | Tăng tính hướng đối tượng, giảm phình to Service | **P2** | Lớp `Booking` ở Domain chỉ chứa các getter/setter thuộc tính dữ liệu. |
