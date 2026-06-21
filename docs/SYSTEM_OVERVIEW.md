# 🌍 Tổng Quan Hệ Thống (System Overview)

## 1. Mục tiêu hệ thống
Dự án **Parking Building Management System (PBMS)** là một hệ thống quản lý bãi đỗ xe thông minh dành cho các tòa nhà. API này cung cấp các dịch vụ Backend để quản lý không gian đỗ xe, đặt trước chỗ đỗ, đăng ký vé tháng, ghi nhận sự cố, theo dõi thanh toán và phân tích thống kê doanh thu. Mục tiêu của hệ thống là tự động hóa các nghiệp vụ bãi đỗ xe, tăng tính minh bạch, giảm thiểu thời gian chờ đợi và tối ưu hiệu suất sử dụng vị trí đỗ.

## 2. Các chức năng chính (Core Features)
Hệ thống PBMS cung cấp các nhóm chức năng chính sau:
* **Quản lý cấu trúc bãi đỗ xe (Parking Structure Management):**
  * Quản lý thông tin Tòa nhà (`Building`), Tầng (`Floor`), Khu vực (`Zone`), và từng Vị trí đỗ (`ParkingSlot`).
  * Theo dõi trạng thái hoạt động và khả năng lấp đầy của bãi xe theo thời gian thực.
* **Đặt chỗ đỗ xe trước (Booking & Reservation):**
  * Cho phép khách hàng tìm kiếm vị trí đỗ trống và đặt trước (`Booking`).
  * Quản lý các phiên gửi xe (`ParkingSession`) từ lúc xe vào cho tới lúc xe ra.
* **Gói đăng ký tháng (Monthly Subscription):**
  * Quản lý gói đăng ký tháng dành cho cư dân hoặc khách hàng thường xuyên.
  * Hỗ trợ gia hạn, hủy gói và tự động kiểm tra thời hạn sử dụng.
* **Quản lý phương tiện và thẻ xe (Vehicle & Card Management):**
  * Quản lý thẻ gửi xe vật lý (`Card`) – bao gồm thẻ tháng và thẻ lượt.
  * Đăng ký và quản lý danh sách phương tiện (`Vehicle`) gắn liền với chủ thẻ hoặc người dùng.
* **Xử lý sự cố (Incident Management):**
  * Ghi nhận và báo cáo các sự cố xảy ra tại bãi đỗ xe (`Incident`) thông qua phân loại loại sự cố (`IncidentType`).
* **Quản lý danh sách đen (Blacklist Management):**
  * Quản lý danh sách đen các xe hoặc thẻ vi phạm quy định gửi xe.
* **Thanh toán (Payment integration):**
  * Tích hợp cổng thanh toán trực tuyến qua **VNPay** để thanh toán đặt chỗ hoặc gia hạn vé tháng.
  * Hỗ trợ thanh toán trực tiếp/tiền mặt tại quầy.
* **Báo cáo và Thống kê (Revenue & Statistics):**
  * Tổng hợp doanh thu tự động theo ngày/tháng/năm (`RevenueStatistic`).
  * Phân tích doanh thu theo loại phương tiện.

## 3. Các module chính trong mã nguồn
Mã nguồn được phân tách theo mô hình Clean Architecture gồm các module cốt lõi sau:
* **PBMS.API**: Điểm tiếp nhận request từ Client (Controllers, Middlewares, Cấu hình Swagger, Authentication JWT).
* **PBMS.Application**: Lớp chứa nghiệp vụ cốt lõi (Business Logic, Services, Interfaces, DTOs, Mapping).
* **PBMS.Domain**: Lớp chứa các thực thể nghiệp vụ (`Entities`), các quy tắc nghiệp vụ cơ bản, và các giao diện dùng chung (Common Interfaces).
* **PBMS.Infrastructure**: Lớp chịu trách nhiệm kết nối Cơ sở dữ liệu (Entity Framework DbContext, Repositories, Migrations) và các dịch vụ bên ngoài (VNPay, Google Auth).

## 4. Công nghệ sử dụng (Technology Stack)
* **Framework chính:** .NET 10.0 (ASP.NET Core Web API).
* **ORM (Object-Relational Mapping):** Entity Framework Core.
* **Cơ sở dữ liệu:**
  * **Production & Development:** PostgreSQL (Lưu trữ trên Supabase Cloud hoặc PostgreSQL local server).
  * **Parking Session Store (Tùy chọn mô phỏng):** Hỗ trợ cấu hình lưu trữ tạm thời trên bộ nhớ RAM (In-Memory Store) riêng biệt cho tính năng Parking Session khi phát triển và chạy thử nghiệm.
* **Xác thực:** JSON Web Token (JWT) & Google OAuth 2.0.
* **Tích hợp thanh toán:** VNPay Sandbox.
* **Tài liệu hóa API:** Swagger UI.
* **Container hóa:** Docker & Docker Compose.
* **CI/CD:** GitHub Actions.
