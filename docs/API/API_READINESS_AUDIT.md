> 🔍 Audited at commit: 6ba866a — 2026-06-24

# 📋 API Readiness Audit

Báo cáo này đánh giá mức độ sẵn sàng tích hợp với Frontend (FE Readiness) của toàn bộ các API trong hệ thống PBMS, chỉ ra các logic còn thiếu, các lỗi bảo mật/phân quyền, và đưa ra khuyến nghị tối ưu hóa.

---

## 1. Bảng Tổng Hợp Đánh Giá Độ Sẵn Sàng (Readiness Summary)

| Phân hệ / Module | Số Endpoint | Đánh giá chung (FE Readiness) | Mức độ ưu tiên sửa đổi | Các thiếu sót cốt lõi | Khuyến nghị chính |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Auth & Accounts** | 8 | `Ready` | **P2** | Thiếu refresh token. | Bổ sung API làm mới token. |
| **Booking Reservation**| 8 | `Ready` (Đã vá lỗi No-Show) | **P1** | Thiếu API kiểm tra sức chứa khả dụng trước khi đặt chỗ (FE không biết slot trống để hiển thị). | Viết thêm API lấy capacity trống của Building theo khoảng thời gian. |
| **Card Management** | 7 | `Ready` | **P2** | Trạng thái thẻ cứng chưa đồng bộ. | Đã ổn định, FE có thể tích hợp. |
| **Parking Structure** | 27 | `Ready` | **P2** | Không có sơ đồ mặt bằng trực quan. | Bổ sung tọa độ X, Y nếu FE làm sơ đồ họa hình. |
| **Parking Sessions** | 9 | `Ready` (Đã vá lỗi check-in blacklist & đổi slot) | **P1** | Đã tích hợp kiểm tra Blacklist xe/thẻ và chặn đổi slot sai tòa nhà/loại xe. | Ổn định để tích hợp. |
| **Monthly Subscriptions**| 4 | `Needs Minor Improvements`| **P1** | Thiếu cảnh báo hoặc email thông báo sắp hết hạn thanh toán đơn vé tháng. | Tự động cập nhật thời hạn trong background. |
| **Payments & VNPay** | 7 | `Ready` (Đã vá lỗi hiển thị phí & hết hạn) | **P1** | VNPay callback trả trực tiếp JSON thô của VNPay thay vì chuyển hướng FE. | VNPay callback nên redirect FE về trang Payment Success/Fail. |
| **Blacklist** | 6 | `Ready` | **P2** | Chưa có kiểm tra chéo tự động. | Ổn định để tích hợp. |
| **Pricing Policies** | 8 | `Ready` (Đã vá lỗi tính đêm) | **P1** | Luật tính giá đè lên nhau phức tạp nhưng API không có chức năng mô phỏng tính giá thử trước cho khách hàng. | Thêm API tính thử tiền đỗ xe dự kiến dựa trên ngày/giờ truyền vào. |
| **Revenue & Statistics**| 2 | `Ready` | **P2** | Thống kê thô. | Ổn định để tích hợp. |
| **Incident Management**| 5 | `Ready` (Đã vá lỗi trùng lặp & phạt âm) | **P2** | Tự động hóa khóa thẻ khi báo mất và giải quyết xong. | Ổn định để tích hợp. |

*Chú thích:*
* **Ready:** API hoàn chỉnh, đầy đủ dữ liệu, Frontend có thể tích hợp ngay lập tức.
* **Needs Minor Improvements:** Cần sửa đổi nhỏ (thêm trường dữ liệu trả về, thêm điều kiện validate nhỏ).
* **Needs Significant Improvements:** Thiếu logic nghiệp vụ quan trọng, thiếu validate dữ liệu đầu vào hoặc phân quyền bị hở.

---

## 2. Chi Tiết Đánh Giá Theo Từng Phân Hệ (Module Detailed Audit)

### 🛡️ A. Phân hệ Booking (Đặt chỗ trước)

* **Endpoints liên quan:** `POST /api/bookings`, `PUT /api/bookings/{id}`, `DELETE /api/bookings/{id}`, `GET /api/bookings/by-account/{accountId}`.
* **Use Case & Actor:** Driver thực hiện tìm kiếm, đặt chỗ trước và thanh toán cọc.
* **Đánh giá logic hiện tại:**
  * Logic tạo đơn Booking đã check capacity của Building tại [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs#L138) và chặn nếu bãi xe đầy (đã áp dụng tỷ lệ dự phòng `BufferRatio`).
  * Tự động giải phóng capacity khi hủy đơn hoặc hết hạn thanh toán 15 phút.
* **Thiếu sót đối với Frontend:**
  * **Thiếu API GetAvailableCapacity:** Frontend không thể biết trước tòa nhà nào còn chỗ trống vào khung giờ khách hàng muốn đặt đỗ xe để hiển thị lên màn hình tìm kiếm.
  * **Độ ưu tiên:** **P1** (Cần thiết lập trước khi FE thiết kế màn hình đặt chỗ).
  * **Khuyến nghị:** Thêm endpoint `GET /api/buildings/{id}/available-capacity?time=...` để FE kiểm tra trước.

---

### 💳 B. Phân hệ Thẻ xe & Đăng ký tháng (Cards & Monthly Subscriptions)

* **Endpoints liên quan:** `PUT /api/cards/{id}/status`, `POST /api/monthly-subscriptions`.
* **Business Rules hiện có:**
  * Xe ô tô khi đăng ký vé tháng bắt buộc phải được gán một vị trí đỗ cố định (`AssignedSlotId`).
* **Thiếu sót đối với Frontend:**
  * Khi đăng ký vé tháng, API `POST /api/monthly-subscriptions` nhận vào `CreateSubscriptionRequest` chứa `AssignedSlotId`. Tuy nhiên, không có endpoint nào trả về danh sách các slot xe ô tô còn trống chưa được gán cho ai để FE hiển thị danh sách lựa chọn.
  * **Độ ưu tiên:** **P1**.
  * **Khuyến nghị:** Thêm bộ lọc `isAssigned=false` vào API `GET /api/parkingslots` để lọc ra các vị trí đỗ còn trống có thể gán cho thẻ tháng.

---

### 🚗 C. Phiên gửi xe thực tế (Parking Sessions)

* **Endpoints liên quan:** `POST /api/parking-sessions/check-in`, `PATCH /api/parking-sessions/{id}/complete`, `PATCH /api/parking-sessions/{id}/slot`.
* **Logic hiện có:**
  * Kiểm tra xem phương tiện hoặc thẻ đã có phiên gửi xe đang hoạt động (Active Session) hay chưa để chặn gửi trùng.
  * Tự động kiểm tra Blacklist đối với cả xe và thẻ gửi xe khi Check-in.
  * Khi đổi slot, xác thực chặt chẽ xem loại xe của slot mới có phù hợp và slot mới có thuộc cùng một tòa nhà với phiên gửi xe hay không.
* **Thiếu sót đối với Frontend:**
  * Đã vá các lỗ hổng cốt lõi. Sẵn sàng tích hợp.
  * **Độ ưu tiên:** **P2**.

---

### 💳 D. Tích hợp thanh toán trực tuyến (Payments & VNPay)

* **Endpoints liên quan:** `POST /api/payments`, `GET /api/payments/callback`.
* **Logic hiện có:**
  * `ProcessVNPayIPNAsync` tại [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs) đã làm rất tốt việc xác thực chữ ký bảo mật từ VNPay và cập nhật tự động trạng thái đơn hàng.
  * Tự động hủy các giao dịch `PENDING` cũ khi tạo giao dịch thanh toán mới.
  * Giới hạn thời gian thanh toán online trong vòng 15 phút. Quá hạn thanh toán sẽ từ chối giao dịch IPN callback.
  * Ngăn chặn rollback checkout nếu đã có giao dịch được xử lý thành công (`PAID`).
* **Thiếu sót đối với Frontend:**
  * Endpoint `GET /api/payments/callback` là điểm trả kết quả từ VNPay. Hiện tại nó trả trực tiếp kết quả JSON thô.
  * **Độ ưu tiên:** **P1** (Chặn tích hợp luồng thanh toán của FE).
  * **Khuyến nghị:** Cần phân tách thành IPN URL (cho Server) và Return URL (cho Client chuyển hướng về trang Frontend).

---

## 3. Lịch sử Rà soát & Các Vấn đề Đã Giải Quyết (Audit History & Resolved Issues)

Dưới đây ghi nhận tiến độ giải quyết các vấn đề sau chu kỳ vá lỗi lớn:

### ✅ Các Vấn đề Đã Giải Quyết (Resolved Issues)

- **[Resolved - P0] Lỗi tính phí đỗ xe đêm sáng sớm (Overnight Window Bug):** Vá lỗi thuật toán phân đoạn tính phí trong [FeeCalculationService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Pricing/Services/FeeCalculationService.cs), đảm bảo xe đỗ qua đêm được tính tiền chuẩn xác thay vì 0 VNĐ.
- **[Resolved - P0] Thiếu tính tiền phạt sự cố khi Checkout:** Vá logic tính tiền trong [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs) để cộng dồn tiền phạt sự cố đang mở (`IncidentStatus.Open`) vào hóa đơn cần thanh toán.
- **[Resolved - P1] Tính kép dung lượng bãi đỗ khi Booking vào bãi:** Vá logic check-in trong [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs) để tự động liên kết Booking và đổi trạng thái sang `CheckedIn`, giải phóng dung lượng đặt chỗ ngay khi xe vào bãi.
- **[Resolved - P1] Trạng thái Booking `"Completed"` không hợp lệ:** Loại bỏ gán trạng thái `"Completed"` tại thời điểm checkout, đồng bộ hóa quy trình chỉ sử dụng Enum `BookingStatus.cs`.
- **[Resolved - P1] Chưa tự động dọn dẹp đơn giữ chỗ quá hạn check-in:** Nâng cấp hàm dọn dẹp trong [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs) để tự động hủy các đơn quá hạn thanh toán hoặc trễ hẹn check-in, kết hợp triển khai hosted service ngầm [ExpiredBookingCleanupWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/ExpiredBookingCleanupWorker.cs) chạy tự động định kỳ mỗi 5 phút.
- **[Resolved - P1] Tự động hóa cảnh báo sắp hết giờ đỗ xe:** Triển khai hosted service ngầm [OvertimeWarningWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/OvertimeWarningWorker.cs) chạy tự động định kỳ mỗi 1 phút để quét các phiên đỗ xe sắp quá giờ đăng ký (trong vòng 15 phút) và tự động tạo thông báo gửi tới tài khoản Driver.
- **[Resolved - P1] Check-in chưa đối chiếu Blacklist tự động:** Tích hợp kiểm tra danh sách đen (Blacklist) cho cả xe và thẻ gửi xe trong `CheckInAsync` của [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs).
- **[Resolved - P1] Đổi Slot không tương thích:** Thêm logic xác thực khớp `VehicleTypeId` và `BuildingId` khi đổi vị trí đỗ trong `AssignSlotAsync` của [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs).
- **[Resolved - P1] Trùng lặp sự cố & Phạt âm:** Bổ sung ràng buộc không phạt âm, không báo cáo sự cố trên session `COMPLETED`, và ngăn chặn tạo sự cố trùng loại đang mở (`LOST_CARD`, `LATE_CHECKOUT`) trong [IncidentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Incident/Services/IncidentService.cs).
- **[Resolved - P1] Không hiển thị phí khi StartCheckout & Hủy thanh toán PENDING cũ**: Trả về trực tiếp số tiền cần thanh toán trong DTO ở `StartCheckoutAsync` và tự động dọn dẹp các thanh toán `PENDING` cũ trước khi tạo giao dịch thanh toán mới trong [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs).
- **[Resolved - P1] Giới hạn thời gian thanh toán online & Chặn rollback checkout**: Giới hạn thời gian IPN VNPay trong vòng 15 phút và chặn Rollback checkout khi đã thanh toán thành công (`PAID`).
- **[Resolved - P1] Tỷ lệ giữ chỗ dự phòng BufferRatio không hợp lệ**: Ràng buộc `BufferRatio` của `VehicleType` luôn thuộc [0, 100]% trong [VehicleTypeService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Vehicle/Services/VehicleTypeService.cs).
- **[Resolved - P1] Tự động khóa thẻ khi báo mất và giải quyết xong**: Cập nhật thẻ sang trạng thái `Lost` khi báo mất thẻ RFID và sang `Blocked` (khóa thẻ vĩnh viễn) khi giải quyết xong sự cố mất thẻ.

### 🔍 Các Vấn đề Còn Lại (Remaining Issues)

- **[Remaining - P1]** Thiếu API `GetAvailableCapacity` hỗ trợ Frontend kiểm tra chỗ trống bãi xe theo khung giờ khi đặt chỗ.
- **[Remaining - P1]** VNPay Callback chưa hỗ trợ chuyển hướng (redirect) về URL của Frontend.
