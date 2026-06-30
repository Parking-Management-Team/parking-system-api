# 💡 Cơ Sở Tri Thức (Knowledge Base)

Tài liệu này lưu trữ các thông tin nghiệp vụ tiện ích, tài khoản dùng thử, các liên kết quan trọng và kinh nghiệm thực chiến phát triển dự án PBMS.

---

## 1. Liên Kết Hữu Ích (Useful Links)

* **Swagger API UI (Local):** [http://localhost:5029/swagger](http://localhost:5029/swagger)
* **Cổng thanh toán VNPay Sandbox Test:** [https://sandbox.vnpayment.vn/paymentv2/vpcpay.html](https://sandbox.vnpayment.vn/paymentv2/vpcpay.html)
* **Trình quản lý cơ sở dữ liệu Supabase:** [https://supabase.com](https://supabase.com) (Liên hệ Lead để xin quyền truy cập project `umkabswquswaaheqvmxj`).
* **Trình giải mã JWT Token:** [https://jwt.io](https://jwt.io) (Dùng để kiểm tra payload, quyền hạn, thời gian hết hạn của Token khi debug).

---

## 2. Tài Khoản Sử Dụng Thử (Test Accounts)

Khi hệ thống khởi chạy ở môi trường Development, Database seed sẽ tự sinh ra các tài khoản tương ứng với các vai trò (Roles).

> [!TODO]
> Cần cập nhật chính xác danh sách email và password mặc định sau khi tính năng Seed Account được hoàn thiện đầy đủ trong mã nguồn. Dưới đây là cấu hình tài khoản phát triển đề xuất:
> * **Tài khoản Admin:** `admin@pbms.com` / Mật khẩu: `[REDACTED]` (Mặc định: `Admin@123`)
> * **Tài khoản Staff (Nhân viên bãi xe):** `staff@pbms.com` / Mật khẩu: `[REDACTED]` (Mặc định: `Staff@123`)
> * **Tài khoản Customer (Khách hàng):** `customer@pbms.com` / Mật khẩu: `[REDACTED]` (Mặc định: `Customer@123`)

---

## 3. Các Lệnh Thường Dùng (Cheatsheet Commands)

### A. Quản lý Dự án
* **Khôi phục thư viện:** `dotnet restore`
* **Clean build:** `dotnet clean && dotnet build`
* **Chạy API:** `dotnet run --project src/PBMS.API`

### B. Cơ sở dữ liệu EF Core CLI
* **Cập nhật Database:**
  ```bash
  dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
  ```
* **Thêm Migration mới:**
  ```bash
  dotnet ef migrations add <Tên_Migration> --project src/PBMS.Infrastructure --startup-project src/PBMS.API
  ```
* **Xóa Migration chưa apply:**
  ```bash
  dotnet ef migrations remove --project src/PBMS.Infrastructure --startup-project src/PBMS.API
  ```
* **Tạo Script SQL từ Migrations (để deploy thủ công):**
  ```bash
  dotnet ef migrations script --project src/PBMS.Infrastructure --startup-project src/PBMS.API -o script.sql
  ```

---

## 4. Kinh Nghiệm Thực Chiến Cho Developer Mới

* **Lưu ý về In-Memory Database:** Nếu bạn tắt dự án đi bật lại khi đang ở chế độ In-Memory, toàn bộ dữ liệu bạn vừa nhập qua Swagger (như thêm bãi xe, đặt chỗ,...) sẽ biến mất hoàn toàn. Hãy chuyển sang kết nối PostgreSQL local nếu cần dữ liệu được lưu trữ lâu dài trong quá trình debug giao diện Front-end.
* **Xử lý Soft Delete:** Các Entity trong hệ thống hầu hết kế thừa từ `ISoftDeletable`. Khi gọi hàm xóa, dữ liệu không thực sự mất đi trong DB mà cột `IsDeleted` sẽ chuyển thành `true`. Khi viết các câu truy vấn Entity Framework mới, hãy chú ý điều kiện lọc `IsDeleted == false` (hoặc kiểm tra Global Query Filter đã được cấu hình trong DbContext chưa).
* **Kiểm tra Log lỗi:** Nếu API trả về lỗi `500 Internal Server Error`, hãy theo dõi log trên cửa sổ Terminal của API hoặc cấu hình ghi log ra file để bắt được chi tiết Exception ném ra từ Middleware xử lý lỗi tập trung.
