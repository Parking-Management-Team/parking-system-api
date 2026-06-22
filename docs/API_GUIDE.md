# 🔌 Hướng Dẫn API (API Guide)

Tài liệu này cung cấp hướng dẫn cách tích hợp và sử dụng các đầu API của dự án PBMS.

---

## 1. Danh Sách Các Controllers đầy đủ

Hệ thống cung cấp các API thông qua các RESTful Controllers dưới đây (nằm tại thư mục [PBMS.API/Controllers](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers)):

| Controller | Vai trò nghiệp vụ |
| :--- | :--- |
| `AuthController` | Đăng nhập tài khoản, làm mới Token, đăng ký, đăng nhập mạng xã hội (Google OAuth). |
| `AccountsController` | Quản trị tài khoản, cập nhật thông tin người dùng, đổi mật khẩu. |
| `BookingsController` | Quản lý đặt chỗ trước (Booking Reservation) của khách gửi xe. |
| `VehicleTypeController` | CRUD các loại xe (Xe Máy, Ô tô,...). |
| `VehiclesController` | Đăng ký và quản lý xe của người dùng. |
| `CardController` | Quản lý vòng đời thẻ vật lý (đăng ký mới, báo mất thẻ, khóa thẻ). |
| `BuildingsController` | Quản lý thông tin các tòa nhà thuộc quyền quản lý. |
| `FloorsController` | Quản lý thông tin tầng trong tòa nhà. |
| `ZonesController` | Quản lý phân khu đỗ xe. |
| `ParkingSlotsController` | Quản lý chi tiết vị trí đỗ xe. |
| `ParkingSessionsController` | Xử lý sự kiện xe vào bãi, xe ra bãi, theo dõi phiên gửi xe thời gian thực. |
| `MonthlySubscriptionsController`| Quản lý đăng ký thẻ tháng, gia hạn thẻ tháng. |
| `PaymentsController` | Tích hợp cổng thanh toán VNPay và lịch sử giao dịch. |
| `IncidentController` | Ghi nhận các báo cáo sự cố hư hại, mất thẻ từ nhân viên. |
| `IncidentTypeController` | Xem phân loại các loại sự cố hệ thống. |
| `BlacklistController` | Quản lý danh sách đen các phương tiện/thẻ bị cấm vào bãi. |
| `PricingPoliciesController` | Quản lý cấu hình chính sách giá đỗ xe theo giờ/ngày. |
| `RevenueController` | Xuất dữ liệu báo cáo thống kê doanh thu bãi đỗ xe. |

---

## 2. Xác Thực & Phân Quyền (Authentication & Authorization)

Hệ thống sử dụng cơ chế **JWT (JSON Web Token)** để bảo vệ các endpoints nhạy cảm.

### Cách gửi Token từ Client:
Mỗi yêu cầu HTTP gửi đến API cần đính kèm JWT Token trong tiêu đề (Header):
```http
Authorization: Bearer <your_jwt_token>
```

### Cơ chế Login Google OAuth:
* API cung cấp endpoint đăng nhập Google. Client gửi Google ID Token nhận được từ Google SDK, API sẽ xác thực và cấp phát JWT token hệ thống tương ứng nếu tài khoản hợp lệ.

---

## 3. Định Dạng Phản Hồi Chuẩn (Base Response Format)

Tất cả các API phản hồi về Client đều tuân thủ một cấu trúc dữ liệu nhất quán định nghĩa bởi lớp `BaseResponse<T>`:

### A. Phản hồi thành công (Success Response - HTTP 200/201)
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Xe Máy",
    "isActive": true
  },
  "message": "Lấy thông tin loại xe thành công.",
  "errorCode": null,
  "errors": null
}
```

### B. Phản hồi lỗi hệ thống / Nghiệp vụ (Error Response - HTTP 400/404/500)
```json
{
  "success": false,
  "data": null,
  "message": "Không tìm thấy loại xe yêu cầu.",
  "errorCode": "NOT_FOUND",
  "errors": null
}
```

### C. Lỗi xác thực dữ liệu đầu vào (Validation Error - HTTP 400)
```json
{
  "success": false,
  "data": null,
  "message": "Dữ liệu đầu vào không hợp lệ.",
  "errorCode": "VALIDATION_ERROR",
  "errors": {
    "Name": ["Tên loại xe không được để trống."]
  }
}
```

---

## 4. Tài Liệu Hóa Tự Động (Swagger UI)

Swagger UI được tích hợp sẵn để hỗ trợ việc chạy thử trực tiếp trên trình duyệt.
* **Đường dẫn local:** `http://localhost:5029/swagger`
* **Cách sử dụng:**
  1. Truy cập đường dẫn Swagger.
  2. Bấm nút **Authorize** ở góc trên bên phải, nhập `Bearer <your_token>` để thiết lập quyền truy cập cho toàn bộ phiên làm việc.
  3. Chọn API cần test, bấm **Try it out**, điền thông số và bấm **Execute**.
