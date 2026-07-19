> 🔍 Audited at commit: 6ba866a — 2026-06-24
> 🔄 Updated at audit: 2026-06-27 (Sau đợt refactor cấu hình động, slot booking time-query, pricing cleanup, check-in protection, dynamic capacity API và VNPay Redirect)

# 📋 API Readiness Audit

Báo cáo này đánh giá mức độ sẵn sàng tích hợp với Frontend (FE Readiness) của toàn bộ các API trong hệ thống PBMS, chỉ ra các logic còn thiếu, các lỗi bảo mật/phân quyền, và đưa ra khuyến nghị tối ưu hóa.

---

## 1. Bảng Tổng Hợp Đánh Giá Độ Sẵn Sàng (Readiness Summary)

| Phân hệ / Module | Số Endpoint | Đánh giá chung (FE Readiness) | Mức độ ưu tiên sửa đổi | Các thiếu sót cốt lõi | Khuyến nghị chính |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Auth & Accounts** | 8 | `Ready` | **P2** | Thiếu refresh token. | Bổ sung API làm mới token. |
| **Booking Reservation**| 9 | `Ready` (Đã bổ sung API Available Capacity theo thời gian) | **P2** | Đã hoàn chỉnh các luồng kiểm tra sức chứa và dọn dẹp đặt chỗ. | Sẵn sàng để FE tích hợp luồng tìm kiếm và đặt xe. |
| **Card Management** | 7 | `Ready` | **P2** | Trạng thái thẻ cứng chưa đồng bộ. | Đã ổn định, FE có thể tích hợp. |
| **Parking Structure** | 28 | `Ready` (Đã hỗ trợ Query Slot theo thời gian đặt) | **P2** | Không có sơ đồ mặt bằng trực quan. | Bổ sung tọa độ X, Y nếu FE làm sơ đồ họa hình. |
| **Parking Sessions** | 9 | `Ready` (Đã có logic bảo vệ slot/zone đặt trước khi check-in) | **P2** | Đã vá logic bảo vệ vị trí đỗ tại repository lúc check-in. | Ổn định để tích hợp. |
| **Monthly Subscriptions**| 4 | `Needs Minor Improvements`| **P1** | Thiếu cảnh báo hoặc email thông báo sắp hết hạn thanh toán đơn vé tháng. | Tự động cập nhật thời hạn trong background. |
| **Payments & VNPay** | 8 | `Ready` (Đã bổ sung chuyển hướng VNPay Return về FE) | **P2** | Đã tách biệt IPN (Server-to-Server) và Return URL (Redirect browser về FE). | Sẵn sàng tích hợp luồng thanh toán trọn vẹn. |
| **Blacklist** | 6 | `Ready` | **P2** | Chưa có kiểm tra chéo tự động. | Ổn định để tích hợp. |
| **Pricing Policies** | 9 | `Ready` (Đã có background cleanup tự động) | **P2** | Đã bổ sung cơ chế Background Worker và API dọn dẹp chính sách giá hết hạn. | Sẵn sàng tích hợp. |
| **Revenue & Statistics**| 2 | `Ready` | **P2** | Thống kê thô. | Ổn định để tích hợp. |
| **Incident Management**| 5 | `Ready` (Đã vá lỗi trùng lặp & phạt âm) | **P2** | Tự động hóa khóa thẻ khi báo mất và giải quyết xong. | Ổn định để tích hợp. |

*Chú thích:*
* **Ready:** API hoàn chỉnh, đầy đủ dữ liệu, Frontend có thể tích hợp ngay lập tức.
* **Needs Minor Improvements:** Cần sửa đổi nhỏ (thêm trường dữ liệu trả về, thêm điều kiện validate nhỏ).
* **Needs Significant Improvements:** Thiếu logic nghiệp vụ quan trọng, thiếu validate dữ liệu đầu vào hoặc phân quyền bị hở.

---

## 2. Chi Tiết Đánh Giá Theo Từng Phân Hệ (Module Detailed Audit)

### 🛡️ A. Phân hệ Booking (Đặt chỗ trước)

* **Endpoints liên quan:** `POST /api/bookings`, `PUT /api/bookings/{id}`, `DELETE /api/bookings/{id}`, `GET /api/bookings/by-account/{accountId}`, `GET /api/buildings/{id}/available-capacity`.
* **Use Case & Actor:** Driver thực hiện tìm kiếm bãi đỗ còn chỗ, chọn khung giờ đặt trước, xem sơ đồ vị trí và thanh toán cọc.
* **Đánh giá logic hiện tại:**
  * Cấu hình thời hạn thanh toán cọc (`PaymentDeadlineMinutes`), thời lượng tối thiểu (`MinBookingDurationHours`), v.v., đã được di chuyển sang file cấu hình `appsettings.json`, cho phép điều chỉnh động.
  * Logic tạo đơn Booking đã check capacity của Building tại [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs#L104) và chặn nếu bãi xe đầy.
  * **Đã giải quyết thiếu sót cho FE**: Thêm endpoint `GET /api/buildings/{id}/available-capacity` để FE lấy số chỗ trống khả dụng cho từng loại xe trong khoảng thời gian xác định, phục vụ màn hình kết quả tìm kiếm bãi đỗ tổng quát.
  * Tự động giải phóng capacity khi hủy đơn hoặc hết hạn thanh toán 15 phút.
* **Đánh giá tích hợp:** Sẵn sàng để tích hợp.

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
  * Đã nâng cấp logic check-in (`FindAvailableZoneAsync` và `FindAvailableGeneralSlotAsync` trong [ParkingSessionRepository.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Infrastructure/Repositories/ParkingSessionRepository.cs)) để khi tính toán sức chứa trống của Phân khu/Slot, hệ thống sẽ tính gộp cả những chiếc xe có Booking (`Confirmed`/`Pending` còn hạn) sắp đến trong vòng 30 phút. Điều này ngăn chặn việc xe vãng lai đỗ chiếm vị trí đã được khách đặt trước.
  * Kiểm tra xem phương tiện hoặc thẻ đã có phiên gửi xe đang hoạt động (Active Session) hay chưa để chặn gửi trùng.
  * Tự động kiểm tra Blacklist đối với cả xe và thẻ gửi xe khi Check-in.
  * Khi đổi slot, xác thực chặt chẽ xem loại xe của slot mới có phù hợp và slot mới có thuộc cùng một tòa nhà với phiên gửi xe hay không.
* **Thiếu sót đối với Frontend:**
  * Đã vá các lỗ hổng cốt lõi. Sẵn sàng tích hợp.
  * **Độ ưu tiên:** **P2**.

---

### 💳 D. Tích hợp thanh toán trực tuyến (Payments & VNPay)

* **Endpoints liên quan:** `POST /api/payments`, `GET /api/payments/callback`, `GET /api/payments/vnpay-return`.
* **Logic hiện có:**
  * `ProcessVNPayIPNAsync` tại [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs) đã làm rất tốt việc xác thực chữ ký bảo mật từ VNPay và cập nhật tự động trạng thái đơn hàng.
  * Tự động hủy các giao dịch `PENDING` cũ khi tạo giao dịch thanh toán mới.
  * Giới hạn thời gian thanh toán online trong vòng 15 phút. Quá hạn thanh toán sẽ từ chối giao dịch IPN callback.
  * Ngăn chặn rollback checkout nếu đã có giao dịch được xử lý thành công (`PAID`).
  * **Đã giải quyết thiếu sót cho FE**: Phân tách luồng nhận tin nhắn Server-to-Server ngầm (IPN Callback tại `/api/payments/callback` vẫn giữ nguyên trả JSON chuẩn cho VNPay) và luồng nhận trình duyệt của khách hàng quay lại (Return Callback tại `/api/payments/vnpay-return` tự động redirect trình duyệt về link Frontend cấu hình trong `appsettings.json` kèm các kết quả giao dịch).
* **Đánh giá tích hợp:** Sẵn sàng để tích hợp.

---

## 3. Lịch sử Rà soát & Các Vấn đề Đã Giải Quyết (Audit History & Resolved Issues)

Dưới đây ghi nhận tiến độ giải quyết các vấn đề sau chu kỳ vá lỗi lớn:

### ✅ Các Vấn đề Đã Giải Quyết (Resolved Issues)

* **[New Resolved - 2026-06-27] Thiếu API GetAvailableCapacity theo thời gian:** Bổ sung endpoint `GET /api/buildings/{id}/available-capacity` trả về số lượng chỗ trống khả dụng cho từng loại xe trong một tòa nhà dựa trên tổng sức chứa hiệu dụng, số xe đang đỗ thực tế và số lượng bookings trùng lịch.
* **[New Resolved - 2026-06-27] VNPay Callback chưa hỗ trợ chuyển hướng về Frontend:** Triển khai endpoint `GET /api/payments/vnpay-return` tự động chuyển hướng trình duyệt của người dùng về trang Frontend kết quả thanh toán cấu hình trong `appsettings.json` kèm các query parameters.
* **[New Resolved - 2026-06-27] Hằng số cấu hình Booking bị gán cứng:** Chuyển đổi các cấu hình nghiệp vụ đặt chỗ (`MinBookingMinutes`, `MinBookingDurationHours`, `PaymentDeadlineMinutes`, `CheckinGracePeriodMinutes`) từ dạng hằng số gán cứng (`const`) trong [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs) sang đọc động thông qua `IConfiguration` với fallback an toàn.
* **[New Resolved - 2026-06-27] Hiển thị sai trạng thái trống của Slot khi đã bị đặt trước:** Cập nhật endpoint `GET /api/parkingslots/zone/{zoneId}` để nhận tham số thời gian và trả về cờ `IsReserved = true` cho các slot đã bị giữ chỗ bởi Booking (`Confirmed`/`Pending` còn hạn) trùng khung giờ.
* **[New Resolved - 2026-06-27] Tích tụ nhiều chính sách giá Active cùng lúc trên DB:** Bổ sung Background Service [ExpiredPricingPolicyCleanupWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/ExpiredPricingPolicyCleanupWorker.cs) chạy tự động 12 giờ một lần để chuyển đổi các chính sách Active quá hạn sang `Expired`. Đồng thời thêm endpoint dọn dẹp thủ công `POST /api/pricing-policies/cleanup`.
* **[New Resolved - 2026-06-27] Xe vãng lai chiếm chỗ đỗ xe đã đặt trước:** Cải tiến logic check-in phân bổ Zone/Slot vật lý, trừ bớt đi những vị trí đã bị Booking giữ chỗ sắp đến (trong vòng 30 phút) để bảo vệ quyền lợi của khách hàng đã đặt cọc trước.
* **[Resolved] Lỗi tính phí đỗ xe đêm sáng sớm (Overnight Window Bug):** Vá lỗi thuật toán phân đoạn tính phí trong [FeeCalculationService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Pricing/Services/FeeCalculationService.cs), đảm bảo xe đỗ qua đêm được tính tiền chuẩn xác thay vì 0 VNĐ.
* **[Resolved] Thiếu tính tiền phạt sự cố khi Checkout:** Vá logic tính tiền trong [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs) để cộng dồn tiền phạt sự cố đang mở (`IncidentStatus.Open`) vào hóa đơn cần thanh toán.
* **[Resolved] Tính kép dung lượng bãi đỗ khi Booking vào bãi:** Vá logic check-in trong [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs) để tự động liên kết Booking và đổi trạng thái sang `CheckedIn`, giải phóng dung lượng đặt chỗ ngay khi xe vào bãi.
* **[Resolved] Trạng thái Booking `"Completed"` không hợp lệ:** Loại bỏ gán trạng thái `"Completed"` tại thời điểm checkout, đồng bộ hóa quy trình chỉ sử dụng Enum `BookingStatus.cs`.
* **[Resolved] Chưa tự động dọn dẹp đơn giữ chỗ quá hạn check-in:** Nâng cấp hàm dọn dẹp trong [BookingService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Booking/Services/BookingService.cs) để tự động hủy các đơn quá hạn thanh toán hoặc trễ hẹn check-in, kết hợp triển khai hosted service ngầm [ExpiredBookingCleanupWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/ExpiredBookingCleanupWorker.cs) chạy tự động định kỳ mỗi 5 phút.
* **[Resolved] Tự động hóa cảnh báo sắp hết giờ đỗ xe:** Triển khai hosted service ngầm [OvertimeWarningWorker.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.API/Workers/OvertimeWarningWorker.cs) chạy tự động định kỳ mỗi 1 phút để quét các phiên đỗ xe sắp quá giờ đăng ký (trong vòng 15 phút) và tự động tạo thông báo gửi tới tài khoản Driver.
* **[Resolved] Check-in chưa đối chiếu Blacklist tự động:** Tích hợp kiểm tra danh sách đen (Blacklist) cho cả xe và thẻ gửi xe trong `CheckInAsync` của [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs).
* **[Resolved] Đổi Slot không tương thích:** Thêm logic xác thực khớp `VehicleTypeId` và `BuildingId` when đổi vị trí đỗ trong `AssignSlotAsync` của [ParkingSessionService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/ParkingSession/Services/ParkingSessionService.cs).
* **[Resolved] Trùng lặp sự cố & Phạt âm:** Bổ sung ràng buộc không phạt âm, không báo cáo sự cố trên session `COMPLETED`, và ngăn chặn tạo sự cố trùng loại đang mở (`LOST_CARD`, `LATE_CHECKOUT`) trong [IncidentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Incident/Services/IncidentService.cs).
* **[Resolved] Không hiển thị phí khi StartCheckout & Hủy thanh toán PENDING cũ**: Trả về trực tiếp số tiền cần thanh toán trong DTO ở `StartCheckoutAsync` và tự động dọn dẹp các thanh toán `PENDING` cũ trước khi tạo giao dịch thanh toán mới trong [PaymentService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Payment/Services/PaymentService.cs).
* **[Resolved] Giới hạn thời gian thanh toán online & Chặn rollback checkout**: Giới hạn thời gian IPN VNPay trong vòng 15 phút và chặn Rollback checkout khi đã thanh toán thành công (`PAID`).
* **[Resolved] Tỷ lệ giữ chỗ dự phòng BufferRatio không hợp lệ**: Ràng buộc `BufferRatio` của `VehicleType` luôn thuộc [0, 100]% trong [VehicleTypeService.cs](file:///D:/FPT/SWP391/parking-system-api/src/PBMS.Application/Vehicle/Services/VehicleTypeService.cs).
* **[Resolved] Tự động khóa thẻ khi báo mất và giải quyết xong**: Cập nhật thẻ sang trạng thái `Lost` khi báo mất thẻ RFID và sang `Blocked` (khóa thẻ vĩnh viễn) khi giải quyết xong sự cố mất thẻ.

### 🔍 Các Vấn đề Còn Lại (Remaining Issues)

* Không còn vấn đề nghiêm trọng nào chặn (block) việc tích hợp của Frontend.
