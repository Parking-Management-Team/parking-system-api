# 🛠️ Xử Lý Sự Cố (Troubleshooting)

Tài liệu này tổng hợp các lỗi phổ biến mà lập trình viên và người vận hành có thể gặp phải trong quá trình cài đặt, phát triển hoặc triển khai dự án PBMS API và cách khắc phục nhanh.

---

## 1. Lỗi Cổng Localhost Đã Bị Sử Dụng (Port 5029 Already In Use)

* **Hiện tượng:** Khi chạy lệnh `dotnet run` hoặc F5 từ IDE, bạn nhận được lỗi thông báo cổng `5029` đã bị chiếm dụng bởi một tiến trình khác.
* **Nguyên nhân:** Có thể một tiến trình API cũ đang chạy ngầm hoặc một dịch vụ khác trên máy tính của bạn đang mở cổng 5029.
* **Cách xử lý:**
  * **Cách 1: Tắt tiến trình đang chiếm dụng cổng.**
    * *Trên Windows (PowerShell):*
      ```powershell
      Get-Process -Id (Get-NetTCPConnection -LocalPort 5029).OwningProcess | Stop-Process -Force
      ```
  * **Cách 2: Thay đổi cổng chạy của dự án.**
    * Mở file [launchSettings.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Properties/launchSettings.json) và cập nhật giá trị `"applicationUrl": "http://localhost:5029"` thành một cổng khác chưa dùng (ví dụ: `http://localhost:5030`).

---

## 2. Lỗi Không Kết Nối Được Cơ Sở Dữ Liệu (Database Connection Errors)

* **Hiện tượng:** Ứng dụng báo lỗi ném ngoại lệ khi khởi động liên quan đến `Npgsql` hoặc `SqlException`, hoặc không thể truy vấn/ghi dữ liệu.
* **Nguyên nhân:**
  * Địa chỉ IP của máy bạn chưa được đưa vào Whitelist truy cập của Supabase PostgreSQL.
  * Thông tin mật khẩu DB trong [appsettings.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/appsettings.json) bị sai hoặc thay đổi.
  * Dịch vụ PostgreSQL/SQL Server cục bộ chưa được bật.
* **Cách xử lý:**
  * Kiểm tra xem thông tin kết nối có chính xác không.
  * Nếu chạy localDB trên Windows, hãy đảm bảo LocalDB service đã được khởi động:
    ```bash
    sqllocaldb start mssqllocaldb
    ```
  * **Fallback tự động:** Hệ thống có cơ chế tự động chuyển hướng sang DB **In-Memory** nếu kết nối vật lý thất bại. Để bật cưỡng bức chế độ In-Memory, bạn có thể chỉnh cấu hình `"UseInMemoryStore": true` (nếu có định nghĩa) hoặc tạm thời ngắt kết nối internet/xóa connection string trong appsettings để hệ thống tự động fallback.

---

## 3. Lỗi Lệch Migration Cơ Sở Dữ Liệu (Database Migration Out Of Sync)

* **Hiện tượng:** Khi chạy ứng dụng, bạn nhận được lỗi thông báo cấu hình Model của EF Core không trùng khớp với cấu trúc bảng thực tế dưới DB.
* **Nguyên nhân:** Một thành viên khác vừa cập nhật thêm bảng mới hoặc sửa đổi cột ở server mà máy local của bạn chưa cập nhật migration tương ứng.
* **Cách xử lý:**
  1. Kéo code mới nhất từ nhánh `develop` về.
  2. Thực hiện cập nhật database local của bạn:
     ```bash
     dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
     ```
  3. Nếu database local quá lộn xộn và không thể update tuần tự, bạn có thể xóa sạch database cục bộ (hoặc drop db local `pbms_local`) và chạy lại lệnh update trên để EF Core tự tạo lại toàn bộ từ đầu.

---

## 4. Lỗi Thiếu JWT Authentication (Unauthorized 401)

* **Hiện tượng:** Bạn gửi request đến API và nhận về mã trạng thái HTTP 401 Unauthorized dù đã đăng nhập hoặc đã truyền JWT.
* **Nguyên nhân:**
  * Bạn quên chưa thêm tiền tố `Bearer ` trước đoạn mã JWT token khi truyền qua Header `Authorization`.
  * Token đã hết hạn (Expiry time mặc định là 1440 phút).
* **Cách xử lý:**
  * Đảm bảo header có định dạng: `Authorization: Bearer <token_string>` (chú ý khoảng trắng giữa `Bearer` và token).
  * Kiểm tra thời hạn của token hoặc thực hiện gọi API làm mới token (Refresh Token) để nhận token mới.
