# 📋 Các Tính Năng Nghiệp Vụ Còn Thiếu & Đề Xuất Cải Tiến (API Pending Enhancements) - ĐÃ HOÀN THÀNH ✅

Tài liệu này ghi nhận các tính năng nghiệp vụ cốt lõi, luồng xử lý ngoại lệ và tích hợp API đã được hoàn thành trong hệ thống PBMS API.

---

## 🚨 1. Các Vấn Đề Nghiêm Trọng (Chặn luồng nghiệp vụ cốt lõi) - ĐÃ XỬ LÝ ✅

### A. Tích hợp API Hoàn Tiền Cọc khi Hủy Booking (Refund Deposit Flow) - ĐÃ XỬ LÝ ✅
* **Hiện trạng cũ**: Chỉ chuyển trạng thái Booking thành `Cancelled` mà không hoàn tiền cọc.
* **Giải pháp đã triển khai**:
  * Khi hủy đơn booking đủ điều kiện hoàn cọc (hủy trước 60 phút), hệ thống cập nhật trạng thái thanh toán thành `REFUND_PENDING` (Chờ hoàn tiền).
  * Triển khai API `POST /api/payments/{id}/refund` cho phép Admin/Thủ quỹ thực hiện hành động hoàn tiền thực tế và cập nhật trạng thái sang `REFUNDED`.
  * Xem thêm: [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs), [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs), [PaymentsController.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers/PaymentsController.cs).

### B. Luồng Cấp Thẻ Thay Thế khi Mất Thẻ (Lost Card Replacement Flow) - ĐÃ XỬ LÝ ✅
* **Hiện trạng cũ**: Không thể liên kết thẻ mới khi thẻ cũ bị khai báo mất.
* **Giải pháp đã triển khai**:
  * Triển khai API `PATCH /api/parking-sessions/{id}/replace-card` để gán thẻ RFID mới cho lượt đỗ xe, đồng thời tự động tạo sự cố `LOST_CARD` kèm mức phạt tương ứng.
  * Triển khai API `PATCH /api/monthly-subscriptions/{id}/replace-card` để đổi thẻ RFID mới cho vé tháng đang hoạt động.
  * Xem thêm: [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs), [MonthlySubscriptionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/MonthlyCard/Services/MonthlySubscriptionService.cs), [ParkingSessionsController.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers/ParkingSessionsController.cs), [MonthlySubscriptionsController.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers/MonthlySubscriptionsController.cs).

---

## ⚠️ 2. Các Vấn Đề Ảnh Hưởng Đến Vận Hành (Medium Priority) - ĐÃ XỬ LÝ ✅

### A. API Đăng Ký Gia Hạn Vé Tháng (Renew Monthly Subscription) - ĐÃ XỬ LÝ ✅
* **Hiện trạng cũ**: Bắt buộc khách hàng phải đăng ký mới từ đầu khi hết hạn vé tháng.
* **Giải pháp đã triển khai**:
  * Triển khai API `POST /api/monthly-subscriptions/{id}/renew` tự động tính tiền theo cấu hình giá hiện hành và cộng thêm 30 ngày sử dụng vào ngày hết hạn.
  * Xem thêm: [MonthlySubscriptionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/MonthlyCard/Services/MonthlySubscriptionService.cs), [MonthlySubscriptionsController.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers/MonthlySubscriptionsController.cs).

### B. Kiểm Tra Blacklist Tự Động khi Tạo Đặt Chỗ (Booking Blacklist Verification) - ĐÃ XỬ LÝ ✅
* **Hiện trạng cũ**: Tạo booking thành công nhưng sau đó bị barie từ chối ở bãi do xe/tài khoản bị blacklist.
* **Giải pháp đã triển khai**:
  * Tích hợp kiểm tra Blacklist cho cả phương tiện (`VehicleId`) và tài khoản sở hữu (`AccountId`) ngay trong bước tạo Booking. Hệ thống từ chối tạo booking nếu thuộc danh sách đen.
  * Xem thêm: [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs).

---

## 💡 3. Tối Ưu Trải Nghiệm & Quản Trị (Low Priority) - ĐÃ XỬ LÝ ✅

### A. API Dashboard Tổng Quan Cho Quản Trị (Dashboard Summary API) - ĐÃ XỬ LÝ ✅
* **Hiện trạng cũ**: Thiếu màn hình thống kê nhanh thời gian thực cho trang chủ quản trị.
* **Giải pháp đã triển khai**:
  * Triển khai endpoint `GET /api/dashboard/summary` trả về các chỉ số nhanh: Số lượt xe đỗ hiện tại, lượt đặt trước hôm nay, tỷ lệ lấp đầy bãi đỗ xe, số lượng sự cố đang xử lý và vé tháng đang kích hoạt.
  * Xem thêm: [DashboardService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Common/Services/DashboardService.cs), [DashboardController.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Controllers/DashboardController.cs).
