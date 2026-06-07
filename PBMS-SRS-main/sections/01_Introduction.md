# 1. Introduction

## 1.1 Purpose

Tài liệu này mô tả yêu cầu nghiệp vụ và yêu cầu chức năng của hệ thống quản lý tòa nhà gửi xe.

Mục tiêu của tài liệu là giúp team hiểu rõ:

- Hệ thống phục vụ ai.
- Hệ thống giải quyết vấn đề gì.
- Hệ thống có những chức năng nào.
- Luồng xử lý xe máy, ô tô, booking, thẻ tháng và thanh toán diễn ra như thế nào.
- Các business rule chính cần tuân theo khi triển khai.

Tài liệu này có thể dùng làm cơ sở cho BA, developer, tester, PM và stakeholder khi phân tích, phát triển, kiểm thử và
nghiệm thu hệ thống.

---

## 1.2 Scope

### In Scope

| Nhóm phạm vi                      | Nội dung                                                                                                                                                                                                                                                                         |
|-----------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Hardware Simulation               | Không có camera, thẻ vật lý, barrier, cảm biến slot. Tất cả thao tác được nhập hoặc xác nhận thủ công trên web.                                                                                                                                                                  |
| Vehicle Support                   | Hệ thống cho phép cấu hình loại phương tiện ở mức dữ liệu nghiệp vụ. Trong phiên bản hiện tại, hệ thống chỉ kích hoạt và kiểm thử chính cho xe máy và ô tô.<br/>Các loại phương tiện khác như xe đạp, xe điện có thể được bổ sung sau thông qua cấu hình hoặc mở rộng nghiệp vụ. |
| Motorcycle Parking                | Khi check-in, hệ thống hiện gợi ý Zone/Area còn chỗ, không quản lý tới từng slot cụ thể.                                                                                            |
| Car Parking                       | Khi check-in, ô tô được hệ thống gợi ý Zone còn chỗ. Slot cụ thể chỉ áp dụng trong các trường hợp có booking slot hoặc thẻ tháng ô tô được cấp slot riêng.                                                                                                                       |
| Booking                           | Người dùng có thể đặt trước chỗ gửi xe, bắt buộc nhập biển số và đặt cọc phí booking.                                                                                                                                                                                            |
| Motorcycle Booking                | Xe máy booking không chọn Zone/Slot cụ thể. Hệ thống đảm bảo có chỗ khi khách đến đúng thời gian hợp lệ.                                                                                                                                                                         |
| Car Booking                       | Ô tô booking được chọn Slot cụ thể. Slot đó được giữ cho booking trong khung thời gian đặt trước.                                                                                                                                                                                |
| Monthly Card                      | Thẻ tháng xe máy đảm bảo có chỗ, không phân theo Zone/Slot.<br/>Thẻ tháng ô tô được cấp Slot riêng, tương tự như mua quyền sử dụng một slot trong thời hạn thẻ.                                                                                                                  |
| Payment                           | Hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng. Không hỗ trợ thanh toán bằng thẻ.                                                                                                                                                                            |
| Fee Calculation                   | Giá gửi xe được tính theo mô hình Time Window, Base Price, Increment/Block Pricing và Window Cap. Nếu phiên gửi xe đi qua nhiều khung giờ, hệ thống tách phiên theo từng khung giờ và áp dụng bảng giá riêng cho từng khung.                                                     |
| Pricing Policy Configuration      | Manager có thể cấu hình bảng giá theo loại xe, khung giờ, thời lượng cơ bản, giá cơ bản, block tính thêm, giá block phát sinh, cap theo khung giờ, phí phạt và các biến cấu hình liên quan.                                                                                      |
| Rounding Policy                   | Hệ thống hỗ trợ rule làm tròn thời gian phát sinh theo grace period và làm tròn tiền mặt theo đơn vị làm tròn cấu hình. Thanh toán online giữ nguyên giá trị chính xác.                                                                                                          |
| Card & Booking Status Management  | Hệ thống quản lý trạng thái Slot, Card và Booking theo các trạng thái nghiệp vụ đã định nghĩa.                                                                                                                                                                                   |
| Driver Account & Vehicle          | Tài khoản Driver có thể được tạo trước khi có xe. Một tài khoản có thể thêm nhiều xe sau này.                                                                                                                                                                                    |
| Scalability in Business Structure | Có thể mở rộng Building, Floor, Zone, Slot ở mức cấu hình nghiệp vụ.                                                                                                                                                                                                             |
| Parking Session Tracking          | Ghi nhận xe vào, trạng thái đang gửi, khu vực/slot được phân bổ, phí tạm tính.                                                                                                                                                                                                   |
| Concept / Entity / Physical Model | Tài liệu bao gồm phần tổng quan concept relationship, entity summary và physical table model để đồng bộ nghiệp vụ với database model.                                                                                                                                             |
| Vehicle Check-out                 | Staff tìm lượt gửi xe, xác nhận xe ra, tính phí, thu tiền.                                                                                                                                                                                                                       |
| Exception Handling                | Mất mã gửi xe/thẻ mô phỏng, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán.                                                                                                                                                                                              |
| Operation Monitoring              | Manager theo dõi slot/zone, bảng giá, lượt xe, doanh thu, tỷ lệ lấp đầy.                                                                                                                                                                                                         |

### Out of Scope

| Nội dung                      | Lý do                                                              |
|-------------------------------|--------------------------------------------------------------------|
| Camera nhận diện biển số thật | Không có hardware thực tế.                                         |
| Thẻ xe vật lý thật            | Mã thẻ/mã gửi xe chỉ là mô phỏng trên web.                         |
| Barrier tự động               | Không tích hợp thiết bị cổng thật.                                 |
| Cảm biến slot thật            | Trạng thái chỗ đỗ được cập nhật bằng thao tác hệ thống/người dùng. |
| Thanh toán bằng thẻ ngân hàng | Chỉ hỗ trợ thanh toán online qua ngân hàng, không thanh toán thẻ.  |
| NFR chi tiết                  | Không phân tích trong phạm vi tài liệu này.                        |
| AI allocation nâng cao        | Có thể để optional/RBL, chưa đưa vào core scope.                   |

---

## 1.3 Definitions, Acronyms, Abbreviations

| Thuật ngữ               | Ý nghĩa trong hệ thống này                                                                                                                            |
|-------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| Building                | Một tòa nhà gửi xe.                                                                                                                                   |
| Floor                   | Một tầng trong tòa nhà.                                                                                                                               |
| Zone/Area               | Khu vực đỗ xe, dùng chính cho xe máy.                                                                                                                 |
| Slot                    | Vị trí đỗ cụ thể, dùng chính cho ô tô.                                                                                                                |
| Parking Session         | Một lượt gửi xe từ lúc check-in đến check-out.                                                                                                        |
| Booking                 | Đặt chỗ trước cho một khoảng thời gian.                                                                                                               |
| Monthly Card            | Gói/thẻ tháng cho phép gửi xe theo chính sách định kỳ.                                                                                                |
| Manual Input            | Người dùng nhập dữ liệu bằng tay trên web, thay cho camera/thẻ/hardware thật.                                                                         |
| Pricing Window          | Khung thời gian áp dụng một bảng giá cụ thể, ví dụ khung ngày hoặc khung đêm.                                                                         |
| Base Duration           | Thời lượng cơ bản được tính theo giá cơ bản.                                                                                                          |
| Base Price              | Giá cơ bản áp dụng cho Base Duration.                                                                                                                 |
| Increment Block         | Đơn vị thời gian tính thêm sau khi vượt Base Duration.                                                                                                |
| Increment Price         | Giá phát sinh cho mỗi Increment Block.                                                                                                                |
| Window Cap              | Mức phí tối đa trong một khung giờ riêng biệt. Không áp dụng cho toàn bộ phiên gửi xe.                                                                |
| Grace Period            | Khoảng thời gian ân hạn. Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period thì không tính thêm block mới.                                        |
| Deposit Fee             | Khoản phí khách phải thanh toán trước khi booking được xác nhận. Theo Parking Price, Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành. |
| Payment Timeout         | Thời gian tối đa cho phép chờ thanh toán booking. Nếu quá thời gian này mà chưa thanh toán thành công, booking bị hủy.                                |
| Check-in Grace Time     | Thời gian ân hạn cho phép khách check-in sau giờ booking đã xác nhận.                                                                                 |
| Downgrade               | Việc chuyển vé tháng hết hạn sang trạng thái tính phí như khách vãng lai nếu xe vẫn còn trong bãi.                                                    |
| Cash Rounding           | Quy tắc làm tròn số tiền khi thanh toán tiền mặt.                                                                                                     |
| Online Payment Rounding | Thanh toán online không làm tròn, giữ nguyên giá trị chính xác.                                                                                       |
| Virtual Card Code       | Mã thẻ/mã gửi xe mô phỏng, không phải thẻ vật lý.                                                                                                     |
| Available               | Còn trống.                                                                                                                                            |
| Occupied                | Đang có xe sử dụng.                                                                                                                                   |
| Reserved                | Đã được booking hoặc giữ chỗ.                                                                                                                         |
| Maintenance/Locked      | Tạm khóa, không được phân bổ.                                                                                                                         |

---

## 1.4 References

- Parking Building Management System — SRS Analysis Draft.
- PBMS - SRS Updated Sections.
- Software Requirements Specification Template.

---

## 1.5 Document Overview

- Chương 1 mô tả mục đích, phạm vi, thuật ngữ và tài liệu tham chiếu.
- Chương 2 mô tả tổng quan hệ thống.
- Chương 3 mô tả stakeholders, actors và quyền tổng quan.
- Chương 4 mô tả business context, workflow và success criteria.
- Chương 5 mô tả functional requirements.
- Chương 6 mô tả business rules.
- Chương 7 mô tả các điểm chính sách đã chốt.
- Chương 8 mô tả Concept, Entity và Physical Model đã đồng bộ với SRS.

---
