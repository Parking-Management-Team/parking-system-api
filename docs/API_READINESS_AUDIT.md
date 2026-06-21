# 📋 API Readiness Audit

Báo cáo này đánh giá mức độ sẵn sàng tích hợp với Frontend (FE Readiness) của toàn bộ các API trong hệ thống PBMS, chỉ ra các logic còn thiếu, các lỗi bảo mật/phân quyền, và đưa ra khuyến nghị tối ưu hóa.

---

## 1. Bảng Tổng Hợp Đánh Giá Độ Sẵn Sàng (Readiness Summary)

| Phân hệ / Module | Số Endpoint | Đánh giá chung (FE Readiness) | Mức độ ưu tiên sửa đổi | Các thiếu sót cốt lõi | Khuyến nghị chính |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Auth & Accounts** | 8 | `Ready` | **P2** | Thiếu refresh token. | Bổ sung API làm mới token. |
| **Booking Reservation**| 8 | `Needs Minor Improvements`| **P1** | Thiếu API kiểm tra sức chứa khả dụng trước khi đặt chỗ (FE không biết slot trống để hiển thị). | Viết thêm API lấy capacity trống của Building theo khoảng thời gian. |
| **Card Management** | 7 | `Ready` | **P2** | Trạng thái thẻ cứng chưa đồng bộ. | Đã ổn định, FE có thể tích hợp. |
| **Parking Structure** | 27 | `Ready` | **P2** | Không có sơ đồ mặt bằng trực quan. | Bổ sung tọa độ X, Y nếu FE làm sơ đồ họa hình. |
| **Parking Sessions** | 9 | `Needs Minor Improvements`| **P1** | Check-in xe lượt không kiểm tra xem xe có nằm trong Blacklist hay không. | Tích hợp kiểm tra Blacklist trước khi cho Check-in. |
| **Monthly Subscriptions**| 4 | `Needs Minor Improvements`| **P1** | Thiếu cảnh báo hoặc email thông báo sắp hết hạn thanh toán đơn vé tháng. | Tự động cập nhật thời hạn trong background. |
| **Payments & VNPay** | 2 | `Ready` | **P1** | VNPay callback trả trực tiếp JSON thô của VNPay thay vì chuyển hướng FE. | VNPay callback nên redirect FE về trang Payment Success/Fail. |
| **Blacklist** | 6 | `Ready` | **P2** | Chưa có kiểm tra chéo tự động. | Ổn định để tích hợp. |
| **Pricing Policies** | 8 | `Needs Minor Improvements`| **P1** | Luật tính giá đè lên nhau phức tạp nhưng API không có chức năng mô phỏng tính giá thử trước cho khách hàng. | Thêm API tính thử tiền đỗ xe dự kiến dựa trên ngày/giờ truyền vào. |
| **Revenue & Statistics**| 2 | `Ready` | **P2** | Thống kê thô. | Ổn định để tích hợp. |

*Chú thích:*
* **Ready:** API hoàn chỉnh, đầy đủ dữ liệu, Frontend có thể tích hợp ngay lập tức.
* **Needs Minor Improvements:** Cần sửa đổi nhỏ (thêm trường dữ liệu trả về, thêm điều kiện validate nhỏ).
* **Needs Significant Improvements:** Thiếu logic nghiệp vụ quan trọng, thiếu validate dữ liệu đầu vào hoặc phân quyền bị hở.

---

## 2. Chi Tiết Đánh Giá Theo Từng Phân Hệ (Module Detailed Audit)

### 🛡️ A. Phân hệ Booking (Đặt chỗ trước) - *Mới thêm*

* **Endpoints liên quan:** `POST /api/bookings`, `PUT /api/bookings/{id}`, `DELETE /api/bookings/{id}`, `GET /api/bookings/by-account/{accountId}`.
* **Use Case & Actor:** Driver thực hiện tìm kiếm, đặt chỗ trước và thanh toán cọc.
* **Đánh giá logic hiện tại:**
  * Logic tạo đơn Booking đã check capacity của Building tại [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs#L61) và chặn nếu bãi xe đầy.
  * Tự động giải phóng capacity khi hủy đơn hoặc hết hạn thanh toán 15 phút.
* **Thiếu sót đối với Frontend:**
  * **Thiếu API GetAvailableCapacity:** Frontend không thể biết trước tòa nhà nào còn chỗ trống vào khung giờ khách hàng muốn đặt đỗ xe để hiển thị lên màn hình tìm kiếm. Nếu bắt khách hàng chọn bãi xe rồi bấm đặt mới trả về lỗi "Building capacity full" (409 Conflict) thì trải nghiệm người dùng (UX) sẽ rất kém.
  * **Độ ưu tiên:** **P1** (Cần thiết lập trước khi FE thiết kế màn hình đặt chỗ).
  * **Khuyến nghị:** Thêm endpoint `GET /api/buildings/{id}/available-capacity?time=...` để FE kiểm tra trước.

---

### 💳 B. Phân hệ Thẻ xe & Đăng ký tháng (Cards & Monthly Subscriptions)

* **Endpoints liên quan:** `PUT /api/cards/{id}/status`, `POST /api/monthly-subscriptions`.
* **Business Rules hiện có:**
  * Xe ô tô khi đăng ký vé tháng bắt buộc phải được gán một vị trí đỗ cố định (`AssignedSlotId`) tại [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs#L141).
* **Thiếu sót đối với Frontend:**
  * Khi đăng ký vé tháng, API `POST /api/monthly-subscriptions` nhận vào `CreateSubscriptionRequest` chứa `AssignedSlotId`. Tuy nhiên, không có endpoint nào trả về danh sách các slot xe ô tô còn trống chưa được gán cho ai để FE hiển thị danh sách lựa chọn (Dropdown list) cho khách hàng.
  * **Độ ưu tiên:** **P1**.
  * **Khuyến nghị:** Thêm bộ lọc `isAssigned=false` vào API `GET /api/parkingslots` để lọc ra các vị trí đỗ còn trống có thể gán cho thẻ tháng.

---

### 🚗 C. Phiên gửi xe thực tế (Parking Sessions)

* **Endpoints liên quan:** `POST /api/parking-sessions/check-in`, `PATCH /api/parking-sessions/{id}/complete`.
* **Logic hiện có:**
  * Kiểm tra xem phương tiện hoặc thẻ đã có phiên gửi xe đang hoạt động (Active Session) hay chưa để chặn gửi trùng.
* **Thiếu sót đối với Frontend:**
  * **Không check Blacklist khi Check-in:** Mặc dù hệ thống có `BlacklistController` để cấm các xe vi phạm, nhưng trong logic `CheckInAsync` của [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs) chưa thực hiện gọi sang `BlacklistRepository` để kiểm tra và từ chối check-in đối với xe hoặc thẻ nằm trong danh sách đen.
  * **Độ ưu tiên:** **P1** (Lỗ hổng nghiệp vụ).
  * **Khuyến nghị:** Thêm bước kiểm tra chéo danh sách đen trước khi tạo phiên Check-in mới.

---

### 💳 D. Tích hợp thanh toán trực tuyến (Payments & VNPay)

* **Endpoints liên quan:** `POST /api/payments`, `GET /api/payments/callback`.
* **Logic hiện có:**
  * `ProcessVNPayIPNAsync` tại [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs#L225) đã làm rất tốt việc xác thực chữ ký bảo mật từ VNPay và cập nhật tự động trạng thái đơn hàng (đối với cả Booking, Session đỗ xe và Vé tháng).
* **Thiếu sót đối với Frontend:**
  * Endpoint `GET /api/payments/callback` là điểm trả kết quả từ VNPay. Hiện tại nó trả trực tiếp kết quả JSON thô (`{ RspCode = "00", Message = "Confirm success" }`). Điều này đúng kỹ thuật IPN của VNPay, nhưng nếu người dùng thanh toán trên Web/Mobile App, trình duyệt sẽ bị dừng lại ở trang JSON thô này thay vì được chuyển hướng (redirect) về lại trang thành công/thất bại của Frontend.
  * **Độ ưu tiên:** **P1** (Chặn tích hợp luồng thanh toán của FE).
  * **Khuyến nghị:** Cần phân tách thành 2 API:
    1. **IPN URL (Dành cho Server-to-Server):** Giữ nguyên như hiện tại để cập nhật DB ẩn.
    2. **Return URL (Dành cho Client-to-Server):** Đọc kết quả VNPay và thực hiện lệnh chuyển hướng trình duyệt (Redirect HTTP 302) về địa chỉ Frontend (ví dụ: `http://localhost:3000/payment-success?orderId=...`).
