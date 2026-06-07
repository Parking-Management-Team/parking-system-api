# 2. Overall Description

## 2.1 Product Perspective

Hệ thống là một web app quản lý tòa nhà gửi xe, hoạt động như một hệ thống mô phỏng quy trình vận hành bãi xe. Thay vì
tích hợp camera, thẻ vật lý, barrier hoặc cảm biến, toàn bộ dữ liệu như biển số, loại xe, mã thẻ, thao tác vào/ra, thanh
toán sẽ được nhập hoặc xác nhận thủ công trên web.

Hệ thống thay thế một phần quy trình thủ công hiện tại bằng quy trình số hóa: ghi nhận xe vào/ra, kiểm tra chỗ trống,
phân bổ chỗ đỗ, tính phí, thanh toán và theo dõi trạng thái bãi xe.

---

## 2.2 Product Functions

| Function Group                      | Mô tả                                                                                                                                                                |
|-------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Parking Structure Management        | Quản lý Building, Floor, Zone, Slot ở mức cấu hình.                                                                                                                  |
| Driver Account & Vehicle Management | Cho phép tạo tài khoản Driver không cần có xe ban đầu; một tài khoản có thể thêm nhiều xe.                                                                           |
| Vehicle Check-in                    | Tiếp nhận xe vào bãi bằng nhập liệu thủ công.                                                                                                                        |
| Parking Allocation                  | Khi check-in, xe máy và ô tô vãng lai được gợi ý theo Zone còn chỗ; slot được dùng cho ô tô booking hoặc ô tô thẻ tháng.                                                 |
| Booking Management                  | Booking yêu cầu chọn Building trước, nhập biển số, thanh toán Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành, thời gian đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng.                          |
| Monthly Card Management             | Thẻ tháng xe máy đảm bảo có chỗ; thẻ tháng ô tô được cấp Slot riêng.                                                                                                 |
| Parking Session Tracking            | Theo dõi lượt gửi xe từ check-in đến check-out.                                                                                                                      |
| Vehicle Check-out                   | Tính phí, xác nhận thanh toán và kết thúc lượt gửi xe.                                                                                                               |
| Payment Management                  | Hỗ trợ tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán thẻ.                                                                                |
| Fee Calculation                     | Tính phí theo thời gian thực tế sử dụng, loại xe, từng pricing window, base duration, base price, block phát sinh và window cap.                                     |
| Pricing Policy Management           | Cho phép Manager cấu hình bảng giá, khung giờ, block tính phí, cap từng khung giờ, grace period, phí phạt và rule làm tròn.                                          |
| System State Management             | Quản lý trạng thái Slot, Card và Booking theo các trạng thái nghiệp vụ như AVAILABLE, RESERVED, OCCUPIED, ACTIVE, COMPLETED, PENDING, CONFIRMED, CANCELLED, EXPIRED. |
| Exception Handling                  | Xử lý mất mã, sai biển số, quá hạn, sai khu vực, chưa thanh toán.                                                                                                    |
| Operation Monitoring                | Manager xem trạng thái bãi, doanh thu, lượt xe, tỷ lệ lấp đầy.                                                                                                       |

---

## 2.3 User Classes and Characteristics

| User Class            | Mục tiêu sử dụng                                                              | Đặc điểm                                      |
|-----------------------|-------------------------------------------------------------------------------|-----------------------------------------------|
| Parking Manager       | Quản lý cấu trúc bãi xe, bảng giá, tình trạng vận hành, báo cáo.              | Dùng để giám sát và cấu hình.                 |
| Parking Staff         | Check-in, check-out, thu phí, xử lý ngoại lệ.                                 | Dùng thường xuyên trong vận hành hằng ngày.   |
| Parking User / Driver | Xem thông tin bãi, booking, đăng ký thẻ tháng, theo dõi lượt gửi, thanh toán. | Có thể dùng self-service nếu hệ thống hỗ trợ. |
| System Administrator  | Quản lý tài khoản, phân quyền, cấu hình hệ thống.                             | Dùng để quản trị hệ thống.                    |

---

## 2.4 Operating Environment

Hệ thống chạy trên web browser. Người dùng thao tác qua giao diện web.

Không yêu cầu:

- Camera thật.
- Thẻ vật lý thật.
- Máy quét thẻ.
- Cảm biến slot.
- Barrier tự động.
- Thiết bị IoT.

Có thể triển khai theo hướng:

- Web app nội bộ.
- Web app demo/mô phỏng.
- Web app có thể mở rộng tích hợp hardware trong tương lai, nhưng không thuộc scope hiện tại.

---

## 2.5 Constraints

| Constraint                       | Mô tả                                                                                                                                                                                   |
|----------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| No real hardware                 | Tất cả dữ liệu được nhập thủ công hoặc mô phỏng.                                                                                                                                        |
| Manual operation                 | Staff phải nhập/xác nhận biển số, mã thẻ, check-in, check-out.                                                                                                                          |
| Vehicle type limitation          | Phiên bản hiện tại chỉ kích hoạt nghiệp vụ cho xe máy và ô tô.<br/>Hệ thống không hard-code loại xe, nhưng các loại xe khác chưa được phân tích nghiệp vụ chi tiết trong phiên bản này. |
| Motorcycle allocation level      | Xe máy chỉ được phân bổ tới Zone/Area, không phân bổ slot cụ thể.                                                                                                                       |
| Check-in allocation level        | Khi check-in, cả xe máy và ô tô đều được hệ thống gợi ý theo Zone còn chỗ.                                                                                                              |
| Motorcycle booking level         | Xe máy booking phải chọn Building trước; có thể chọn Zone nếu hệ thống cho phép. Nếu không chọn Zone, hệ thống tự chọn Zone phù hợp trong Building đã chọn.                              |
| Car booking level                | Ô tô booking được chọn Slot cụ thể.                                                                                                                                                     |
| Monthly card guarantee           | Thẻ tháng xe máy đảm bảo có chỗ nhưng không giữ Zone/Slot cụ thể; thẻ tháng ô tô giữ Slot riêng.                                                                                        |
| Monthly card capacity protection | Hệ thống phải giữ phần sức chứa/slot riêng cho nhóm thẻ tháng để không làm mất quyền đảm bảo chỗ.                                                                                       |
| Pricing model                    | Hệ thống áp dụng mô hình Time Window, Base Price, Increment/Block Pricing và Window Cap.                                                                                                |
| Window cap limitation            | Window Cap chỉ áp dụng cho từng khung giờ riêng biệt, không áp dụng cho toàn bộ phiên gửi xe.                                                                                           |
| 24/7 operation                   | Hệ thống vận hành 24/7 và không reset parking session khi qua ngày mới.                                                                                                                 |
| Dynamic configuration            | Các biến giá, khung giờ, block, grace period, timeout, penalty và rounding phải được cấu hình động, không hardcode vào source code.                                                     |
| Cash rounding                    | Thanh toán tiền mặt có thể làm tròn theo đơn vị cấu hình.                                                                                                                               |
| Online payment rounding          | Thanh toán online không làm tròn, giữ nguyên giá trị chính xác.                                                                                                                         |
| Booking time limit               | Booking chỉ được tạo nếu thời gian đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng so với thời điểm thanh toán cọc thành công.                                                            |
| Booking deposit policy           | Booking yêu cầu thanh toán Deposit Fee trước. Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.                                                                          |
| Booking cancellation policy      | Nếu khách hủy trước thời điểm booking ít nhất 1 tiếng, hệ thống hoàn cọc.                                                                                                               |
| Booking no-show policy           | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và khách mất cọc.                                                                                                      |
| Payment method limitation        | Hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán bằng thẻ.                                                                                   |
| Expandable structure             | Hệ thống phải cho phép mở rộng Building, Floor, Zone, Slot ở mức quản lý dữ liệu nghiệp vụ.                                                                                             |

---

## 2.6 Assumptions and Dependencies

| ID    | Assumption                                                                                                                                          |
|-------|-----------------------------------------------------------------------------------------------------------------------------------------------------|
| A-001 | Staff là người thao tác chính trong check-in/check-out nếu người dùng không tự thực hiện được.                                                      |
| A-002 | Biển số xe được nhập thủ công, nên hệ thống cần kiểm tra trùng, sai hoặc thiếu biển số.                                                             |
| A-003 | Mã gửi xe/mã thẻ là mã mô phỏng trên web, không phải thẻ vật lý.                                                                                    |
| A-004 | Xe máy được quản lý theo sức chứa Zone/Area, không theo slot cụ thể.                                                                                |
| A-005 | Ô tô khi gửi thực tế cần được gán slot cụ thể để tránh xung đột vị trí.                                                                             |
| A-006 | Khi check-in, xe máy và ô tô đều được hệ thống gợi ý theo Zone còn chỗ.                                                                             |
| A-007 | Xe máy booking phải chọn Building trước; Zone có thể do Driver chọn hoặc do hệ thống tự chọn trong Building đã chọn.                                |
| A-008 | Ô tô booking phải chọn Building trước; Slot/Zone có thể do Driver chọn hoặc do hệ thống tự chọn trong Building đã chọn.                              |
| A-009 | Booking chỉ hợp lệ sau khi thanh toán cọc thành công.                                                                                               |
| A-010 | Booking phải được đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng tính từ thời điểm thanh toán cọc thành công.                                        |
| A-011 | Booking phải có thời gian dự kiến gửi xe để hệ thống tính tiền cọc.                                                                                 |
| A-012 | Tiền cọc booking bằng giá của block đầu tiên theo bảng giá hiện hành.                                                                               |
| A-013 | Nếu khách hủy booking trước giờ booking ít nhất 1 tiếng, hệ thống hoàn cọc.                                                                         |
| A-014 | Nếu khách đến trễ quá 45 phút so với giờ booking, booking bị hủy và khách mất cọc.                                                                  |
| A-015 | Nếu khách đến sớm hơn giờ booking, hệ thống cho phép check-in sớm nếu còn chỗ phù hợp. Thời gian gửi xe bắt đầu tính từ lúc khách thực tế check-in. |
| A-016 | Nếu khách dùng đúng trong khoảng thời gian đã đặt, khi check-out khách thanh toán phần còn lại sau khi trừ cọc.                                     |
| A-017 | Nếu khách gửi vượt quá thời gian đã đặt, phần vượt được tính phí theo chính sách gửi xe thông thường.                                               |
| A-018 | Thẻ tháng xe máy đảm bảo có chỗ nhưng không phân theo Zone/Slot cụ thể.                                                                             |
| A-019 | Thẻ tháng ô tô được cấp Slot riêng trong thời hạn thẻ, tương tự như mua quyền sử dụng slot.                                                         |
| A-020 | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày trong thời gian còn hiệu lực.                                                                      |
| A-021 | Một Driver Account có thể tồn tại mà chưa có xe. Xe có thể được thêm sau.                                                                           |
| A-022 | Một Driver Account có thể quản lý nhiều xe.                                                                                                         |
| A-023 | Online payment là thanh toán thật qua ngân hàng, không phải mô phỏng và không phải thanh toán thẻ.                                                  |
| A-024 | Phí gửi xe có thể thay đổi theo loại xe, số giờ, khung giờ sáng/tối, qua đêm và nhiều ngày.                                                         |
| A-025 | Hệ thống tính phí theo thời gian thực tế sử dụng.                                                                                                   |
| A-026 | Nếu phiên gửi xe đi qua nhiều pricing window, hệ thống tách phiên theo từng khung giờ và áp dụng bảng giá riêng cho từng khung.                     |
| A-027 | Window Cap chỉ giới hạn phí trong từng pricing window, không giới hạn toàn bộ session.                                                              |
| A-028 | Hệ thống vận hành 24/7 và không reset session khi qua ngày mới.                                                                                     |
| A-029 | Nếu vé tháng hết hạn lúc xe vẫn còn trong bãi, hệ thống downgrade quyền lợi vé tháng và bắt đầu tính phí vãng lai từ thời điểm hết hạn.             |
| A-030 | Booking chưa thanh toán trong thời gian timeout sẽ tự động bị hủy và slot/capacity được trả về trạng thái available.                                |
| A-031 | Nếu khách booking không check-in trong thời gian ân hạn, booking bị hủy và deposit fee không được hoàn trả.                                         |
| A-032 | Nếu khách check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai.                                   |
| A-033 | Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính block mới. Nếu vượt grace period, hệ thống tính thành block mới.        |
| A-034 | Thanh toán tiền mặt áp dụng cash rounding nếu có số lẻ, discount hoặc VAT.                                                                          |
| A-035 | Thanh toán online không làm tròn số tiền.                                                                                                           |

---
