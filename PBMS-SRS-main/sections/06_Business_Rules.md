# 6. Business Rules

## 6.1 Parking Structure Rules

| Rule ID | Rule                                                                |
|---------|---------------------------------------------------------------------|
| BR-001  | Building có thể có nhiều Floor.                                     |
| BR-002  | Floor có thể có nhiều Zone.                                         |
| BR-003  | Zone có thể dùng cho xe máy hoặc ô tô tùy cấu hình.                 |
| BR-004  | Xe máy được quản lý theo sức chứa của Zone.                         |
| BR-005  | Ô tô được quản lý theo từng Slot.                                   |
| BR-006  | Zone/Slot bị khóa hoặc bảo trì không được dùng để check-in/booking. |

---

## 6.2 Hardware Simulation Rules

| Rule ID   | Rule                                  | Cách xử lý                                                                          |
|-----------|---------------------------------------|-------------------------------------------------------------------------------------|
| BR-HW-001 | Hệ thống không có camera thật.        | Biển số được nhập thủ công trên web.                                                |
| BR-HW-002 | Hệ thống không có thẻ vật lý thật.    | Dùng mã gửi xe/mã thẻ mô phỏng.                                                     |
| BR-HW-003 | Hệ thống không có cảm biến slot thật. | Trạng thái Zone/Slot được cập nhật qua thao tác check-in/check-out/booking/manager. |
| BR-HW-004 | Hệ thống không có barrier thật.       | Trạng thái cho phép ra/vào chỉ là trạng thái nghiệp vụ trong hệ thống.              |

---

## 6.3 Driver Account & Vehicle Rules

| Rule ID    | Rule                                                                        | Cách xử lý                                                         |
|------------|-----------------------------------------------------------------------------|--------------------------------------------------------------------|
| BR-ACC-001 | Tài khoản Driver có thể được tạo mà chưa cần đăng ký xe.                    | Cho phép account tồn tại với danh sách xe rỗng.                    |
| BR-ACC-002 | Một Driver Account có thể có nhiều xe.                                      | Driver được thêm nhiều biển số/xe vào tài khoản.                   |
| BR-ACC-003 | Xe có thể được thêm sau khi tài khoản đã được tạo.                          | Cung cấp chức năng Add Vehicle.                                    |
| BR-ACC-004 | Khi booking hoặc đăng ký thẻ tháng, hệ thống bắt buộc có thông tin biển số. | Nếu tài khoản chưa có xe, yêu cầu thêm xe hoặc nhập biển số trước. |

---

## 6.4 Parking Allocation Rules

| Rule ID      | Rule                                                                                                   | Cách xử lý                                                                     |
|--------------|--------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------|
| BR-ALLOC-001 | Khi check-in, xe máy được gợi ý theo Zone/Area còn chỗ.                                                | Hệ thống tìm Zone còn capacity phù hợp.                                        |
| BR-ALLOC-002 | Khi check-in, ô tô vãng lai được gợi ý theo Zone còn chỗ.                                              | Hệ thống tìm Zone còn capacity cho ô tô.                                       |
| BR-ALLOC-003 | Ô tô booking được gán theo Slot đã đặt trước.                                                          | Hệ thống hiển thị Slot đã booking khi check-in.                                |
| BR-ALLOC-004 | Ô tô thẻ tháng được gán theo Slot riêng đã cấp.                                                        | Hệ thống hiển thị Slot riêng của xe khi check-in.                              |
| BR-ALLOC-005 | Zone/Slot đang bảo trì hoặc tạm khóa không được phân bổ.                                               | Hệ thống loại khỏi danh sách gợi ý.                                            |
| BR-ALLOC-006 | Sức chứa dành cho thẻ tháng không được cấp hết cho khách vãng lai hoặc booking.                        | Hệ thống giữ lại capacity/slot pool cho nhóm thẻ tháng.                        |
| BR-ALLOC-007 | Hệ thống phải tránh trạng thái lấp đầy 100% nếu điều đó làm mất quyền đảm bảo chỗ của khách thẻ tháng. | Khi capacity chạm ngưỡng giới hạn, hệ thống từ chối nhận thêm walk-in/booking. |

---

## 6.5 Booking Rules

| Rule ID     | Rule                                                                                                  | Cách xử lý                                                                                 |
|-------------|-------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| BR-BOOK-001 | Booking bắt buộc nhập biển số xe.                                                                     | Nếu thiếu biển số, không cho tạo booking.                                                  |
| BR-BOOK-002 | Booking chỉ hợp lệ sau khi thanh toán cọc thành công.                                                 | Trạng thái ban đầu là Pending; sau khi cọc thành công chuyển thành Confirmed.       |
| BR-BOOK-003 | Booking phải được đặt trước tối thiểu 1 tiếng tính từ thời điểm thanh toán cọc thành công.            | Nếu thời gian dự kiến vào bãi nhỏ hơn 1 tiếng sau khi thanh toán, hệ thống từ chối booking. |
| BR-BOOK-004 | Booking chỉ được đặt trước tối đa 8 tiếng tính từ thời điểm thanh toán cọc thành công.                | Nếu thời gian dự kiến vào bãi lớn hơn 8 tiếng sau khi thanh toán, hệ thống từ chối booking. |
| BR-BOOK-006 | Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành. | Hệ thống xác định bảng giá theo loại xe và thời điểm booking để tính deposit. |
| BR-BOOK-007 | Booking phải chọn Building trước. | Hệ thống chỉ kiểm tra Zone/Slot trong Building đã chọn. |
| BR-BOOK-008 | Xe máy booking có thể chọn Zone hoặc để hệ thống tự chọn Zone. | Hệ thống giữ capacity phù hợp trong Building đã chọn. |
| BR-BOOK-009 | Ô tô booking có thể chọn Slot hoặc để hệ thống tự chọn Zone/Slot. | Hệ thống giữ Slot nếu đã xác định Slot cụ thể. |
| BR-BOOK-010 | Booking không được dùng phần capacity/slot đã giữ cho nhóm thẻ tháng.                                 | Hệ thống kiểm tra quota monthly card trước khi xác nhận booking.                           |
| BR-BOOK-010 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, khách được hoàn cọc.                         | Hệ thống chuyển booking sang Cancelled và tạo trạng thái Refund/Refunded.                  |
| BR-BOOK-011 | Nếu khách hủy booking trễ hơn thời hạn được hoàn cọc, khách mất cọc.                                  | Hệ thống chuyển booking sang Cancelled và giữ cọc.                                         |
| BR-BOOK-012 | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và mất cọc.                          | Hệ thống chuyển booking sang No-show/Cancelled và giữ cọc.                                 |
| BR-BOOK-013 | Nếu khách đến đúng hạn hoặc trong thời gian trễ cho phép, booking được dùng để tạo parking session.   | Hệ thống chuyển booking thành session khi check-in thành công.                             |
| BR-BOOK-014 | Nếu khách đến sớm hơn giờ booking, hệ thống cho phép check-in sớm nếu còn chỗ phù hợp.                | Thời gian gửi xe bắt đầu tính từ thời điểm check-in thực tế.                               |
| BR-BOOK-015 | Nếu khách gửi xe trong phạm vi thời gian đã đặt, khách thanh toán phần phí còn lại sau khi trừ cọc.   | Khi check-out, hệ thống thu phần còn lại, thường là 70% phí dự kiến.                       |
| BR-BOOK-016 | Nếu khách gửi vượt quá thời gian đã đặt, phần vượt được tính phí theo chính sách gửi xe thông thường. | Hệ thống cộng phí vượt giờ vào tổng phí cần thanh toán.                                    |
| BR-BOOK-017 | Một biển số không nên có nhiều booking active trong cùng khoảng thời gian.                            | Hệ thống kiểm tra trùng lịch booking theo biển số.                                         |
| BR-BOOK-018 | Booking chưa thanh toán trong booking_payment_timeout_minutes sẽ tự động bị hủy. | Hệ thống chuyển booking sang CANCELLED và trả slot/capacity về AVAILABLE. |
| BR-BOOK-019 | Sau khi thanh toán booking thành công, slot/capacity được giữ cho booking. | Với ô tô booking slot, Slot chuyển sang RESERVED. |
| BR-BOOK-020 | Nếu khách không check-in trong checkin_grace_minutes, booking bị hủy tự động. | Hệ thống chuyển booking sang EXPIRED hoặc CANCELLED và không hoàn Deposit Fee. |
| BR-BOOK-021 | Nếu khách check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai. | Hệ thống cộng phí overtime vào tổng phí cần thanh toán. |

---

## 6.6 Monthly Card Rules

| Rule ID      | Rule                                                                              | Cách xử lý                                                                                 |
|--------------|-----------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------|
| BR-MONTH-001 | Thẻ tháng xe máy phải đảm bảo luôn có chỗ cho khách.                              | Hệ thống giữ capacity phù hợp cho nhóm xe máy thẻ tháng.                                   |
| BR-MONTH-002 | Thẻ tháng xe máy không phân theo Zone/Slot cụ thể.                                | Khi xe vào bãi, hệ thống gợi ý Zone còn chỗ.                                               |
| BR-MONTH-003 | Thẻ tháng ô tô được cấp Slot riêng.                                               | Hệ thống gắn xe ô tô thẻ tháng với một Slot cụ thể trong thời hạn thẻ.                     |
| BR-MONTH-004 | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày.                                 | Cho phép nhiều parking session trong ngày nếu thẻ còn hiệu lực và không có session đang mở. |
| BR-MONTH-005 | Một xe không được có nhiều thẻ tháng còn hiệu lực trùng thời gian.                | Hệ thống kiểm tra biển số trước khi đăng ký/gia hạn.                                       |
| BR-MONTH-006 | Thẻ tháng chỉ được kích hoạt sau khi thanh toán thành công.                       | Nếu chưa thanh toán, trạng thái là Pending.                                         |
| BR-MONTH-007 | Không được cấp thêm thẻ tháng xe máy nếu vượt khả năng đảm bảo chỗ.               | Hệ thống từ chối đăng ký và báo hết quota thẻ tháng xe máy.                                |
| BR-MONTH-008 | Không được cấp thêm thẻ tháng ô tô nếu không còn Slot phù hợp.                    | Hệ thống từ chối đăng ký và báo hết slot thẻ tháng ô tô.                                   |
| BR-MONTH-009 | Capacity/Slot dành cho thẻ tháng phải được ưu tiên hơn khách vãng lai và booking. | Khi tính chỗ trống cho walk-in/booking, hệ thống trừ phần reserved pool của thẻ tháng.     |
| BR-MONTH-010 | Vé tháng áp dụng cho biển số đăng ký cố định. | Hệ thống kiểm tra biển số khi check-in để xác định quyền lợi vé tháng. |
| BR-MONTH-011 | Vé tháng được thanh toán trả trước theo chu kỳ. | Vé chỉ active sau khi thanh toán thành công. |
| BR-MONTH-012 | Vé tháng có số ngày hiệu lực cấu hình. | Hệ thống xác định ngày bắt đầu và ngày hết hạn. |
| BR-MONTH-013 | Vé tháng có thể cấu hình quyền gửi qua đêm. | Nếu không cho phép qua đêm, hệ thống xử lý phát sinh theo chính sách. |
| BR-MONTH-014 | Mỗi vé tháng chỉ áp dụng cho một xe đã đăng ký. | Không cho dùng cùng một vé tháng cho nhiều xe. |
| BR-MONTH-015 | Hệ thống quét vé tháng hết hạn lúc 00:00 mỗi ngày. | Nếu vé hết hạn và xe vẫn còn trong bãi, hệ thống downgrade. |
| BR-MONTH-016 | Khi vé tháng bị downgrade, xe bắt đầu bị tính phí vãng lai từ thời điểm hết hạn. | Áp dụng bảng giá vãng lai cho đến khi xe rời bãi. |
| BR-MONTH-017 | Sau khi gia hạn thành công, quyền lợi vé tháng được kích hoạt lại ngay lập tức. | Hệ thống cập nhật lại trạng thái active. |

---

## 6.7 Payment Rules

| Rule ID    | Rule                                                                                                               | Cách xử lý                                                                |
|------------|--------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| BR-PAY-001 | Hệ thống hỗ trợ thanh toán tiền mặt.                                                                               | Staff xác nhận đã thu tiền trên hệ thống.                                 |
| BR-PAY-002 | Hệ thống hỗ trợ thanh toán online thật qua ngân hàng.                                                              | Hệ thống nhận kết quả thanh toán từ kênh ngân hàng/payment gateway.       |
| BR-PAY-003 | Hệ thống không hỗ trợ thanh toán bằng thẻ.                                                                         | Không hiển thị phương thức card payment.                                  |
| BR-PAY-004 | Booking phải thanh toán Deposit Fee trước. | Booking chỉ confirmed khi Deposit Fee được thanh toán thành công.         |
| BR-PAY-005 | Nếu booking bị hủy do khách đến trễ quá 45 phút, khách mất cọc.                                                    | Không hoàn cọc cho trạng thái No-show.                                    |
| BR-PAY-006 | Check-out chỉ hoàn tất khi khoản phí cần thanh toán đã được xác nhận.                                              | Nếu pending, hệ thống chặn check-out.                                     |
| BR-PAY-007 | Thẻ tháng chỉ active sau khi thanh toán thành công.                                                                | Nếu payment failed, thẻ tháng không có hiệu lực.                          |
| BR-PAY-008 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, khách được hoàn cọc.                                      | Hệ thống tạo trạng thái Refund/Refunded cho khoản cọc.                    |
| BR-PAY-009 | Nếu khách đã đặt cọc booking, khi check-out hệ thống trừ cọc khỏi tổng phí cần thanh toán theo chính sách booking. | Khách chỉ thanh toán phần còn lại hoặc phần phát sinh nếu có.             |
| BR-PAY-008 | Booking chưa thanh toán trong booking_payment_timeout_minutes sẽ bị hủy tự động. | Hệ thống chuyển booking sang CANCELLED và trả slot/capacity về AVAILABLE. |
| BR-PAY-009 | Thanh toán tiền mặt áp dụng cash rounding rule nếu cần. | Làm tròn theo cash_rounding_unit và rounding_threshold.                   |
| BR-PAY-010 | Thanh toán online không làm tròn. | Giữ nguyên giá trị chính xác của giao dịch.                               |
| BR-PAY-011 | Nếu cần hoàn Deposit Fee, hệ thống ghi nhận trạng thái refund/refunded. | Áp dụng cho các trường hợp được hoàn theo chính sách booking.             |

---

## 6.8 Fee Calculation Rules

| Rule ID | Rule | Cách xử lý |
|---|---|---|
| BR-FEE-001 | Phí gửi xe phụ thuộc vào loại xe. | Hệ thống áp dụng bảng giá theo vehicle_type. |
| BR-FEE-002 | Phí gửi xe được tính theo thời gian thực tế sử dụng. | Hệ thống tính từ check-in time đến check-out time. |
| BR-FEE-003 | Hệ thống áp dụng mô hình pricing window. | Mỗi khung giờ có bảng giá riêng. |
| BR-FEE-004 | Mỗi pricing window có base duration và base price. | Nếu thời gian nằm trong base duration, áp dụng base price. |
| BR-FEE-005 | Sau khi vượt base duration, hệ thống tính thêm theo increment block. | Mỗi block phát sinh áp dụng increment price. |
| BR-FEE-006 | Nếu session đi qua nhiều pricing window, hệ thống tách session theo từng window. | Mỗi đoạn thời gian được tính theo bảng giá riêng. |
| BR-FEE-007 | Window Cap chỉ áp dụng cho từng pricing window riêng biệt. | Không dùng window cap để giới hạn toàn bộ session. |
| BR-FEE-008 | Hệ thống vận hành 24/7 và không reset session khi qua ngày mới. | Session tiếp tục được tính theo pricing window của ngày tiếp theo. |
| BR-FEE-009 | Deposit Fee của booking bằng giá của block đầu tiên theo bảng giá hiện hành. | Hệ thống tính deposit dựa trên bảng giá tại thời điểm booking. |
| BR-FEE-010 | Nếu khách gửi xe vượt thời gian booking, phần vượt tính theo block pricing của bảng giá vãng lai. | Hệ thống cộng phí overtime vào tổng phí. |
| BR-FEE-011 | Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace_period_minutes, hệ thống không tính block mới. | Không cộng increment price cho phần thời gian này. |
| BR-FEE-012 | Nếu thời gian phát sinh lớn hơn grace_period_minutes, hệ thống tính thành block mới. | Cộng thêm increment price tương ứng. |
| BR-FEE-013 | Thanh toán tiền mặt áp dụng cash rounding nếu có số lẻ, discount hoặc VAT. | Hệ thống làm tròn theo cash_rounding_unit và rounding_threshold. |
| BR-FEE-014 | Thanh toán online không làm tròn. | Hệ thống giữ nguyên giá trị chính xác. |
| BR-FEE-015 | Nếu có phí phạt hoặc phụ phí, hệ thống cộng vào tổng phí trước khi thanh toán. | Áp dụng lost card penalty, wrong zone penalty hoặc phí phát sinh khác nếu có. |

---

## 6.9 Parking Session Rules

| Rule ID | Rule                                                   |
|---------|--------------------------------------------------------|
| BR-028  | Session bắt đầu khi check-in thành công.               |
| BR-029  | Session kết thúc khi check-out và thanh toán hoàn tất. |
| BR-030  | Session đang mở phải giữ Zone/Slot tương ứng.          |
| BR-031  | Driver chỉ được xem thông tin của mình.                |

---

## 6.10 Vehicle Check-out Rules

| Rule ID | Rule                                                                         |
|---------|------------------------------------------------------------------------------|
| BR-032  | Xe chỉ được check-out nếu có session đang mở.                                |
| BR-033  | Check-out phải xác nhận thanh toán trước khi hoàn tất.                       |
| BR-034  | Sau check-out, Zone/Slot được giải phóng.                                    |
| BR-035  | Nếu có thẻ tháng hợp lệ, phí được xử lý theo chính sách thẻ tháng.           |
| BR-036  | Nếu phát sinh lỗi thông tin, Staff phải xử lý exception trước khi cho xe ra. |

---

## 6.11 Exception Handling Rules

| Rule ID | Rule                                                                              |
|---------|-----------------------------------------------------------------------------------|
| BR-042  | Ngoại lệ phải có lý do xử lý.                                                     |
| BR-043  | Một số ngoại lệ cần Manager xác nhận.                                             |
| BR-044  | Xe chưa thanh toán không được hoàn tất check-out, trừ khi Manager xử lý đặc biệt. |
| BR-045  | Sai biển số phải được chỉnh trước khi hoàn tất session.                           |
| BR-046  | Gửi sai khu vực có thể phát sinh cảnh báo hoặc phí tùy chính sách.                |
| BR-052 | Khi Staff chuyển trạng thái vé/mã gửi xe thành LOST, hệ thống áp dụng lost card penalty. |
| BR-053 | Xe mất vé/mã gửi xe chỉ được rời bãi sau khi thanh toán phí gửi xe hiện tại và lost card penalty. |
| BR-054 | Xe đỗ sai khu vực có thể bị áp dụng wrong zone penalty nếu được Staff xác nhận. |
| BR-055 | Phí phạt được cộng vào tổng phí trước khi thanh toán. |

---

## 6.12 Operation Monitoring Rules

| Rule ID | Rule                                            |
|---------|-------------------------------------------------|
| BR-047  | Manager được xem toàn bộ dữ liệu vận hành.      |
| BR-048  | Dashboard phải phân biệt xe máy và ô tô.        |
| BR-049  | Xe máy hiển thị theo Zone capacity.             |
| BR-050  | Ô tô hiển thị theo Slot status.                 |
| BR-051  | Doanh thu dựa trên các giao dịch đã thanh toán. |

---

## 6.13 System State Rules

### Slot Status

| Status | Ý nghĩa |
|---|---|
| AVAILABLE | Slot trống. |
| RESERVED | Slot đã được booking. |
| OCCUPIED | Slot đang có xe. |
| BLOCKED | Slot bị khóa. |

### Card Status

| Status | Ý nghĩa |
|---|---|
| ACTIVE | Vé/session đang hoạt động. |
| COMPLETED | Đã check-out. |
| LOST | Mất vé/mã gửi xe. |
| EXPIRED | Vé hết hạn. |
| DOWNGRADED | Vé tháng bị downgrade. |

### Booking Status

| Status | Ý nghĩa |
|---|---|
| PENDING | Chờ thanh toán. |
| CONFIRMED | Đã xác nhận. |
| CANCELLED | Đã hủy. |
| EXPIRED | Hết hạn check-in. |
| COMPLETED | Đã sử dụng. |

### Monthly Card Status

| Status | Ý nghĩa |
|---|---|
| PENDING | Chờ thanh toán/kích hoạt. |
| ACTIVE | Đang hiệu lực. |
| EXPIRED | Đã hết hạn. |
| DOWNGRADED | Hết hạn khi xe vẫn còn trong bãi và bị chuyển sang tính phí vãng lai. |
| CANCELLED | Đã hủy. |

## 6.14 Configurable Variables Rules

Các biến cấu hình dưới đây phải được quản lý động ở tầng cấu hình nghiệp vụ/application/admin configuration, không hard-code trực tiếp trong source code. Phiên bản physical model hiện tại không tạo thêm table riêng cho nhóm biến này.

| Key | Ý nghĩa |
|---|---|
| day_start_time | Giờ bắt đầu khung ngày. |
| night_start_time | Giờ bắt đầu khung đêm. |
| base_duration_minutes | Thời lượng cơ bản. |
| increment_block_minutes | Kích thước block. |
| grace_period_minutes | Thời gian ân hạn làm tròn block. |
| booking_payment_timeout_minutes | Timeout thanh toán booking. |
| checkin_grace_minutes | Thời gian ân hạn check-in booking. |
| lost_card_penalty | Phí mất vé/mã gửi xe. |
| wrong_zone_penalty | Phí đỗ sai khu. |
| cash_rounding_unit | Đơn vị làm tròn tiền mặt. |
| rounding_threshold | Ngưỡng làm tròn. |

## 6.13 Business Rules Summary

| Rule Group           | Nội dung chính                                                                                                                           |
|----------------------|------------------------------------------------------------------------------------------------------------------------------------------|
| Hardware Simulation  | Không có camera, thẻ thật, cảm biến, barrier; toàn bộ nhập liệu trên web.                                                                |
| Vehicle Type         | Hiện chỉ hỗ trợ xe máy và ô tô.                                                                                                          |
| Motorcycle Parking   | Xe máy được phân bổ theo Zone/Area, không theo Slot cụ thể.                                                                              |
| Car Parking          | Ô tô vãng lai được gợi ý Zone, không bắt buộc Slot; ô tô booking/thẻ tháng dùng Slot đã chọn/cấp hoặc Slot do hệ thống tự chọn.           |
| Check-in             | Chỉ tạo session khi còn chỗ hợp lệ.                                                                                                      |
| Booking | Booking yêu cầu chọn Building trước, nhập biển số, Deposit Fee bằng giá block đầu tiên, đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng; xe máy có thể chọn Zone hoặc để hệ thống tự chọn; ô tô có thể chọn Slot/Zone hoặc để hệ thống tự chọn. |
| Monthly Card         | Thẻ tháng xe máy đảm bảo có chỗ nhưng không phân Zone/Slot; thẻ tháng ô tô được cấp Slot riêng.                                          |
| Booking Cancellation | Hủy trước giờ booking ít nhất 1 tiếng được hoàn cọc; đến trễ quá 45 phút bị hủy booking và mất cọc.                                      |
| Payment              | Hỗ trợ tiền mặt và online qua ngân hàng; không hỗ trợ thanh toán bằng thẻ.                                                               |
| Fee Calculation | Phí gửi xe tính theo Time Window, Base Price, Increment/Block Pricing, Window Cap, Grace Period và trạng thái session 24/7 không reset qua ngày. |
| Exception            | Các lỗi mất mã, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán cần quy trình xử lý.                                              |
| Scale                | Có thể mở rộng Building, Floor, Zone, Slot ở mức cấu hình nghiệp vụ.                                                                     |
| Monthly Card Downgrade | Vé tháng hết hạn lúc xe còn trong bãi sẽ bị downgrade và bắt đầu tính phí vãng lai từ thời điểm hết hạn. |
| Rounding | Tiền mặt có thể làm tròn theo cấu hình; online payment không làm tròn. |
| Penalty | Mất vé/mã gửi xe và đỗ sai khu có thể phát sinh phí phạt. |
| Configurable Rules | Các biến pricing, timeout, grace period, penalty và rounding phải được cấu hình động ở tầng nghiệp vụ/application/admin configuration, không hard-code và không tạo thêm table riêng trong physical model hiện tại. |

---
