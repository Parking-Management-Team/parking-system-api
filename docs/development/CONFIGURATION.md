# ⚙️ Cấu Hình Hệ Thống (Configuration)

Tài liệu này tập trung lưu trữ toàn bộ các tham số cấu hình chính, các cổng chạy, biến môi trường và thiết lập kết nối tới các dịch vụ bên ngoài của dự án PBMS API.

---

## 1. Cổng Chạy & URLs (Localhost Ports)

Các thông số cổng mặc định được cấu hình trong [launchSettings.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Properties/launchSettings.json):
* **Địa chỉ chạy API (HTTP):** `http://localhost:5029`
* **Swagger UI:** `http://localhost:5029/swagger`
* **Swagger JSON Spec:** `http://localhost:5029/swagger/v1/swagger.json`

---

## 2. Các Biến Môi Trường (Environment Variables)

* `ASPNETCORE_ENVIRONMENT`: Xác định môi trường chạy của ứng dụng (`Development` hoặc `Production`).
* `ConnectionStrings__DefaultConnection`: Có thể truyền từ biến môi trường của máy chủ để ghi đè kết nối DB mà không cần sửa file JSON.

---

## 3. Cấu Hợp Cơ Sở Dữ Liệu (Database Connections)

Hệ thống kết nối duy nhất tới cơ sở dữ liệu **PostgreSQL**. Có hai cấu hình tương ứng với từng môi trường:

| Môi trường | Loại DB | Connection String thực tế | Mô tả / Mục đích |
| :--- | :--- | :--- | :--- |
| **Development (Supabase Cloud)** | PostgreSQL | `Host=aws-1-ap-southeast-2.pooler.supabase.com;Database=postgres;Username=[REDACTED];Password=[REDACTED];SSL Mode=Require;Trust Server Certificate=true` | Kết nối DB đám mây dùng chung cho cả đội phát triển test dữ liệu. |
| **Development (Local Host)** | PostgreSQL | `Host=localhost;Port=5432;Database=pbms_local;Username=postgres;Password=[REDACTED]` | Kết nối DB PostgreSQL chạy cục bộ trên máy lập trình viên. |

### Cấu hình mô phỏng ParkingSession Store:
* Trong [appsettings.Development.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/appsettings.Development.json), có cấu hình `"ParkingSession": { "UseInMemoryStore": false }`.
* Nếu cấu hình này chuyển sang `true`, hệ thống sẽ lưu trữ tạm thời các phiên đỗ xe (`ParkingSession`) trên bộ nhớ RAM (In-Memory) để phục vụ chạy mô phỏng nhanh mà không cần lưu trữ xuống PostgreSQL.

---

## 4. Xác Thực JWT (JSON Web Token)

Cơ chế JWT dùng để xác thực và ủy quyền cho người dùng truy cập API. Các thông số trong [appsettings.json](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/appsettings.json):
* **Security Key (Khóa bảo mật):** `[REDACTED]` (Được lưu trữ trong biến cấu hình `Jwt:Key`, thực tế tại file appsettings là `A_Super_Secret_Key_For_JWT_Auth_System_PBMS_Project_2026_SWP391`)
* **Issuer (Nhä phát hành):** `PBMS`
* **Audience (Đối tượng sử dụng):** `PBMSUsers`
* **ExpiryInMinutes (Thời gian hết hạn):** `1440` phút (tương đương 24 giờ).

---

## 5. Dịch Vụ Tích Hợp Bên Thứ Ba (External Services)

### A. Google OAuth 2.0 (Đăng nhập bằng Google)
* **ClientId:** `[REDACTED].apps.googleusercontent.com` (Thực tế là `768808098768-vop4tnm5u22h8stb6464bqtogse2rqvm.apps.googleusercontent.com`)

### B. VNPay (Cổng thanh toán Sandbox)
Dùng để thanh toán phí đặt chỗ đỗ xe hoặc gia hạn thẻ tháng.
* **TmnCode:** `[REDACTED]` (Mã website đăng ký với VNPay, thực tế là `5GH8KI9L`)
* **HashSecret:** `[REDACTED]` (Khóa bảo mật dùng để tạo chữ ký dữ liệu, thực tế là `SZ2DWD9030XB1RB85D6J4CBTGD4SJWFZ`)
* **BaseUrl:** `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html` (Đường dẫn cổng thanh toán thử nghiệm VNPay)
* **ReturnUrl:** `http://localhost:5029/api/payments/callback` (Địa chỉ API nhận kết quả phản hồi từ VNPay sau khi thanh toán)

---

## 🔒 Hướng Dẫn Bảo Mật Khi Đưa Lên Production

> [!WARNING]
> Tuyệt đối không được commit các thông tin nhạy cảm (như mật khẩu DB thật, JWT Key thật, VNPay HashSecret thật) lên GitHub.
> Đối với môi trường Production, hãy sử dụng cơ chế **Environment Variables** hoặc dịch vụ quản lý secret (như AWS Secrets Manager, Azure Key Vault, Supabase Secrets) để ghi đè các cấu hình nhạy cảm này.
