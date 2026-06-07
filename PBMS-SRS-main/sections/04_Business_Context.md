# 4. Business Context

## 4.1 Problem Statement

Hiện tại, nghiệp vụ gửi xe trong tòa nhà nhiều tầng có nhiều điểm dễ sai sót: xe ra/vào liên tục, cần kiểm soát chỗ
trống, cần tính phí, cần xử lý mất vé/mã gửi xe, quá hạn, sai thông tin xe, xe gửi sai khu vực và xe chưa thanh toán.
Nếu quản lý thủ công, bãi xe dễ bị ùn ứ, sai lệch dữ liệu, khó kiểm soát sức chứa và khó đối soát doanh thu.

Trong phiên bản này, vì không có hardware thật, hệ thống cần mô phỏng nghiệp vụ bằng web app nhưng vẫn phải giữ đúng
logic vận hành: xe vào phải có session, xe phải được phân bổ đúng khu vực/slot, booking và thẻ tháng không được làm vượt
sức chứa, xe ra phải được thanh toán và cập nhật trạng thái.

---

## 4.2 Business Goals

| Goal ID | Business Goal                                                                         |
|---------|---------------------------------------------------------------------------------------|
| BG-001  | Chuẩn hóa quy trình xe vào/ra bằng web app.                                           |
| BG-002  | Giảm sai sót khi ghi nhận biển số, mã gửi xe, giờ vào/ra, phí gửi xe.                 |
| BG-003  | Kiểm soát được chỗ trống theo Zone cho xe máy và Slot cho ô tô.                       |
| BG-004  | Hỗ trợ đặt chỗ trước để người dùng chủ động kế hoạch gửi xe.                          |
| BG-005  | Hỗ trợ thẻ tháng nhưng không vượt quá khả năng phục vụ của bãi.                       |
| BG-006  | Hỗ trợ thanh toán tiền mặt và online qua ngân hàng; không hỗ trợ thanh toán bằng thẻ. |
| BG-007  | Cho phép mở rộng mô hình bãi xe khi có thêm Building, Floor, Zone, Slot.              |
| BG-008  | Giúp Manager theo dõi vận hành, doanh thu, lượt xe, tỷ lệ lấp đầy.                    |

---

## 4.3 Success Criteria

| Criteria ID | Success Criteria                                                                                       |
|-------------|--------------------------------------------------------------------------------------------------------|
| SC-001      | Driver có thể tạo tài khoản mà chưa cần thêm xe.                                                       |
| SC-002      | Một Driver Account có thể thêm nhiều xe.                                                               |
| SC-003      | Booking bắt buộc nhập biển số.                                                                         |
| SC-004      | Booking chỉ confirmed sau khi thanh toán cọc thành công.                                               |
| SC-005      | Booking phải được đặt trước tối thiểu 1 tiếng và tối đa 8 tiếng tính từ lúc thanh toán cọc thành công. |
| SC-006      | Xe máy booking phải chọn Building trước; Zone có thể do người dùng chọn hoặc do hệ thống tự chọn.       |
| SC-007      | Ô tô booking phải chọn Building trước; Slot/Zone có thể do người dùng chọn hoặc do hệ thống tự chọn.    |
| SC-008      | Booking yêu cầu thanh toán Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.            |
| SC-009      | Booking được hoàn cọc nếu khách hủy trước giờ booking ít nhất 1 tiếng.                                 |
| SC-010      | Booking bị hủy và mất cọc nếu khách đến trễ quá 45 phút.                                               |
| SC-011      | Thẻ tháng xe máy đảm bảo có chỗ nhưng không phân theo Zone/Slot cụ thể.                                |
| SC-012      | Thẻ tháng ô tô được cấp Slot riêng trong thời hạn thẻ.                                                 |
| SC-013      | Thẻ tháng không giới hạn số lượt vào/ra mỗi ngày.                                                      |
| SC-014      | Hệ thống không cho cấp thêm thẻ tháng nếu vượt khả năng đảm bảo chỗ.                                   |
| SC-015      | Hệ thống hỗ trợ thanh toán tiền mặt và online qua ngân hàng.                                           |
| SC-016      | Hệ thống không hiển thị thanh toán bằng thẻ.                                                           |
| SC-017      | Phí gửi xe được tính theo giờ, loại xe, khung giờ sáng/tối, qua đêm và nhiều ngày.                     |
| SC-018      | Hệ thống không cấp hết chỗ cho walk-in/booking nếu phần còn lại cần giữ cho khách thẻ tháng.           |
| SC-019      | Staff có thể tạo lượt gửi xe bằng nhập thủ công biển số, loại xe, mã gửi xe.                           |
| SC-020      | Xe máy được hệ thống gợi ý Zone/Area còn trống.                                                        |
| SC-021      | Ô tô vãng lai được hệ thống gợi ý Zone còn trống; ô tô booking/thẻ tháng dùng Slot đã chọn/cấp hoặc Slot do hệ thống tự chọn. |
| SC-022      | Khi check-out, hệ thống tính được phí và ghi nhận thanh toán tiền mặt hoặc online.                     |
| SC-023      | Sau khi xe ra, Zone/Slot được cập nhật lại trạng thái còn trống.                                       |
| SC-024      | Manager có thể xem tình trạng chỗ đỗ, lượt xe và doanh thu.                                            |
| SC-025      | Có thể thêm Building/Floor/Zone/Slot mới mà không làm thay đổi luồng nghiệp vụ chính.                  |
| SC-026 | Hệ thống tính phí theo Time Window, Base Price, Increment/Block Pricing và Window Cap. |
| SC-027 | Nếu session đi qua nhiều khung giờ, hệ thống tách session và tính phí riêng cho từng khung. |
| SC-028 | Window Cap chỉ áp dụng trong từng khung giờ riêng biệt. |
| SC-029 | Session không bị reset khi qua ngày mới. |
| SC-030 | Hệ thống tự động downgrade vé tháng hết hạn nếu xe vẫn còn trong bãi và tính phí vãng lai từ thời điểm hết hạn. |
| SC-031 | Booking chưa thanh toán sau booking payment timeout sẽ tự động bị hủy. |
| SC-032 | Booking no-show sau check-in grace time sẽ bị hủy và không hoàn deposit fee. |
| SC-033 | Thời gian phát sinh nhỏ hơn hoặc bằng grace period không bị tính block mới. |
| SC-034 | Thanh toán tiền mặt được làm tròn theo cash rounding rule. |
| SC-035 | Thanh toán online giữ nguyên giá trị chính xác, không làm tròn. |

---

## 4.4 Current Workflow

### Trước khi có hệ thống

1. Nhân viên tiếp nhận xe.
2. Nhân viên ghi nhận biển số, loại xe, giờ vào bằng cách thủ công.
3. Nhân viên tự kiểm tra hoặc ước lượng còn chỗ hay không.
4. Người gửi xe tự tìm khu vực/slot.
5. Khi xe ra, nhân viên tìm lại thông tin gửi xe.
6. Nhân viên tính phí.
7. Người gửi xe thanh toán.
8. Nhân viên ghi nhận xe đã rời bãi.
9. Manager tổng hợp doanh thu/lượt xe thủ công hoặc bán thủ công.

### Pain Points

| Pain Point                                 | Ảnh hưởng                                     |
|--------------------------------------------|-----------------------------------------------|
| Khó biết còn chỗ chính xác                 | Dễ nhận xe vượt sức chứa.                     |
| Xe máy và ô tô cần logic phân bổ khác nhau | Dễ gửi sai khu vực.                           |
| Không có trạng thái booking rõ ràng        | Có thể trùng chỗ.                             |
| Thẻ tháng nếu cấp quá nhiều                | Không đảm bảo còn chỗ.                        |
| Ghi nhận thủ công                          | Dễ sai biển số, giờ vào, phí.                 |
| Thanh toán khó đối soát                    | Dễ lệch doanh thu.                            |
| Không có hardware thật                     | Cần cơ chế mô phỏng nhưng vẫn đúng nghiệp vụ. |

---

## 4.5 Target Workflow

### Luồng tổng quát sau khi có hệ thống

1. Staff hoặc Driver mở chức năng tương ứng trên web.
2. Hệ thống hiển thị thông tin bãi xe, chỗ trống, giá và quy định.
3. Người dùng nhập dữ liệu: biển số, loại xe, mã thẻ/mã gửi xe, thời gian booking hoặc thông tin thẻ tháng.
4. Hệ thống kiểm tra điều kiện hợp lệ.
5. Hệ thống phân bổ:
    - Xe máy → Zone/Area còn trống.
    - Ô tô vãng lai → Zone còn trống.
    - Ô tô booking/thẻ tháng → Slot đã chọn/cấp riêng hoặc Slot do hệ thống tự chọn nếu khách không chọn trước.
6. Hệ thống tạo booking, monthly registration hoặc parking session.
7. Khi xe ra, Staff tìm session.
8. Hệ thống tính phí.
9. Người dùng thanh toán tiền mặt hoặc online.
10. Hệ thống cập nhật trạng thái session, Zone/Slot và giao dịch.
11. Manager theo dõi trạng thái vận hành qua màn hình quản lý.

### 4.5.1 Booking Workflow

#### Main Flow

1. Driver đăng nhập, tạo booking.
2. Driver chọn Building muốn gửi xe.
3. Driver chọn loại xe: xe máy hoặc ô tô.
4. Driver nhập/chọn biển số xe.
5. Driver nhập thời gian dự kiến vào bãi.
6. Driver nhập thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
7. Hệ thống kiểm tra thời gian booking:
    - Tối thiểu trước 1 tiếng so với thời điểm thanh toán cọc thành công.
    - Tối đa trước 8 tiếng so với thời điểm thanh toán cọc thành công.
8. Hệ thống xác định bảng giá hiện hành theo loại xe và thời điểm booking.
9. Hệ thống tính Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
10. Hệ thống kiểm tra chỗ khả dụng trong Building đã chọn:
    - Xe máy: Driver có thể chọn Zone nếu hệ thống cho phép. Nếu không chọn Zone, hệ thống tự chọn Zone phù hợp còn capacity.
    - Ô tô: Driver có thể chọn Slot cụ thể. Nếu không chọn Slot/Zone, hệ thống tự chọn Zone/Slot phù hợp còn khả dụng.
11. Hệ thống tạo yêu cầu thanh toán cọc.
12. Driver thanh toán cọc qua ngân hàng.
13. Nếu thanh toán cọc thành công, hệ thống xác nhận booking và chuyển trạng thái booking sang CONFIRMED.
14. Nếu booking giữ Zone/Slot cụ thể, Zone capacity hoặc Slot tương ứng được giữ cho booking.
15. Khi Driver đến đúng thời gian hợp lệ, booking được chuyển thành parking session.

#### Exception Flow

| Trường hợp                                            | Xử lý                                                           |
|-------------------------------------------------------|-----------------------------------------------------------------|
| Thiếu Building                                        | Không cho tạo booking.                                          |
| Thiếu biển số                                         | Không cho tạo booking.                                          |
| Không nhập thời lượng gửi xe hoặc giờ rời bãi dự kiến | Không cho tạo booking vì hệ thống không đủ dữ liệu để tính cọc. |
| Thời gian booking nhỏ hơn 1 tiếng từ lúc thanh toán   | Từ chối booking.                                                |
| Thời gian booking lớn hơn 8 tiếng từ lúc thanh toán   | Từ chối booking.                                                |
| Thanh toán cọc thất bại                               | Booking không được xác nhận.                                    |
| Xe máy không còn capacity đảm bảo                     | Từ chối booking.                                                |
| Ô tô không còn Slot phù hợp                           | Từ chối booking.                                                |
| Khách hủy trước giờ booking ít nhất 1 tiếng           | Hủy booking và hoàn cọc.                                        |
| Khách hủy trễ hơn thời hạn được hoàn cọc              | Hủy booking và không hoàn cọc.                                  |
| Khách đến trễ quá 45 phút                             | Hủy booking/parking reservation và mất cọc.                     |
| Biển số đã có booking active trùng thời gian          | Cảnh báo hoặc từ chối tạo booking mới.                          |
| Quá thời gian thanh toán booking | Hệ thống tự động hủy booking và trả slot/capacity về trạng thái available. |
| Khách không check-in trong thời gian ân hạn | Booking bị hủy tự động, deposit fee không được hoàn trả. |

### 4.5.2 Monthly Card Workflow

#### Main Flow

1. Driver mở chức năng đăng ký thẻ tháng.
2. Driver chọn hoặc thêm xe vào tài khoản.
3. Hệ thống kiểm tra biển số xe và loại xe.
4. Hệ thống kiểm tra thẻ tháng hiện tại của xe.
5. Hệ thống kiểm tra khả năng đảm bảo chỗ theo loại xe:
    - Xe máy: kiểm tra capacity dành cho nhóm thẻ tháng xe máy.
    - Ô tô: kiểm tra Slot còn có thể cấp riêng cho thẻ tháng ô tô.
6. Nếu còn khả năng đảm bảo chỗ, hệ thống tạo yêu cầu thanh toán.
7. Driver thanh toán phí thẻ tháng.
8. Khi thanh toán thành công, hệ thống kích hoạt thẻ tháng.
9. Khi xe máy thẻ tháng vào bãi, hệ thống gợi ý Zone còn chỗ.
10. Khi ô tô thẻ tháng vào bãi, hệ thống dùng Slot riêng đã được cấp cho xe đó.
11. Driver có thể ra/vào nhiều lượt trong ngày nếu thẻ còn hiệu lực và không có session đang mở cùng lúc.
12. Hệ thống lưu thông tin vé tháng theo biển số đăng ký cố định.
13. Hệ thống ghi nhận chu kỳ hiệu lực của vé tháng theo valid duration.
14. Nếu vé tháng được gia hạn thành công trước hoặc sau khi hết hạn, quyền lợi vé tháng được kích hoạt lại ngay sau khi thanh toán thành công.

#### Downgrade Flow

1. Hệ thống quét trạng thái vé tháng vào 00:00 mỗi ngày.
2. Nếu vé tháng đã hết hạn và xe vẫn còn trong bãi, hệ thống chuyển quyền lợi vé tháng sang trạng thái downgraded/transient.
3. Từ thời điểm hết hạn, hệ thống bắt đầu tính phí theo bảng giá vãng lai.
4. Khi xe check-out, khách thanh toán phần phí phát sinh theo bảng giá vãng lai.

#### Exception Flow

| Trường hợp                                | Xử lý                                           |
|-------------------------------------------|-------------------------------------------------|
| Xe chưa được thêm vào tài khoản           | Yêu cầu thêm xe hoặc nhập biển số.              |
| Xe đã có thẻ tháng còn hiệu lực           | Không cho tạo thẻ tháng trùng thời gian.        |
| Hết capacity đảm bảo cho xe máy thẻ tháng | Từ chối đăng ký thẻ tháng xe máy.               |
| Không còn Slot để cấp cho ô tô thẻ tháng  | Từ chối đăng ký thẻ tháng ô tô.                 |
| Thanh toán thất bại                       | Thẻ tháng không được kích hoạt.                 |
| Thẻ tháng hết hạn                         | Không áp dụng quyền lợi thẻ tháng khi check-in. |
| Vé tháng hết hạn khi xe vẫn còn trong bãi | Hệ thống downgrade vé tháng và tính phí vãng lai từ thời điểm hết hạn. |
| Vé tháng không cho phép gửi qua đêm nhưng xe gửi qua đêm | Hệ thống xử lý theo chính sách phát sinh/phạt hoặc chuyển sang tính phí vãng lai. |
| Muốn dùng cùng một vé tháng cho xe khác | Hệ thống từ chối vì mỗi vé tháng chỉ áp dụng cho một xe đã đăng ký. |

### 4.5.3 Check-in Workflow

#### Main Flow

1. Staff mở màn hình Check-in.
2. Staff nhập biển số hoặc mã booking/mã thẻ tháng.
3. Hệ thống xác định loại xe và trạng thái liên quan:
    - Khách vãng lai.
    - Khách có booking.
    - Khách có thẻ tháng.
4. Hệ thống kiểm tra xe có session đang mở hay không.
5. Hệ thống kiểm tra điều kiện chỗ đỗ:
    - Xe máy vãng lai: tìm Zone/Area còn sức chứa.
    - Ô tô vãng lai: tìm Zone còn sức chứa cho ô tô.
    - Xe máy thẻ tháng: đảm bảo có chỗ, không phân theo Zone/Slot cố định.
    - Ô tô thẻ tháng: kiểm tra Slot riêng đã được cấp cho xe.
    - Xe máy booking: kiểm tra booking còn hiệu lực và còn trong thời gian cho phép.
    - Ô tô booking: kiểm tra Slot đã booking còn hiệu lực.
6. Hệ thống gợi ý chỗ đỗ:
    - Xe máy: gợi ý Zone/Area.
    - Ô tô vãng lai: gợi ý Zone.
    - Ô tô booking/thẻ tháng: hiển thị Slot tương ứng.
7. Staff xác nhận check-in.
8. Hệ thống tạo parking session.
9. Hệ thống cập nhật trạng thái chỗ đỗ.

#### Exception Flow

| Trường hợp                           | Xử lý                                                                                             |
|--------------------------------------|---------------------------------------------------------------------------------------------------|
| Biển số trống                        | Yêu cầu nhập biển số.                                                                             |
| Xe đã có session đang mở             | Không cho tạo session mới.                                                                        |
| Booking chưa thanh toán cọc          | Không cho check-in theo booking.                                                                  |
| Booking đã bị hủy do trễ quá 45 phút | Không áp dụng booking, khách mất cọc và chỉ được xử lý như khách mới nếu còn chỗ.                 |
| Khách đến sớm hơn giờ booking        | Cho phép check-in sớm nếu còn chỗ phù hợp; thời gian gửi xe bắt đầu tính từ lúc check-in thực tế. |
| Hết chỗ phù hợp                      | Từ chối check-in.                                                                                 |
| Zone/Slot bị khóa hoặc bảo trì       | Không phân bổ.                                                                                    |

### 4.5.4 Check-out & Payment Workflow

#### Main Flow

1. Staff mở màn hình Check-out.
2. Staff nhập biển số hoặc mã gửi xe.
3. Hệ thống tìm parking session đang mở.
4. Hệ thống tính phí dựa trên:
   - Loại xe.
   - Thời gian thực tế sử dụng.
   - Pricing Window tương ứng.
   - Base Duration.
   - Base Price.
   - Increment Block.
   - Increment Price.
   - Window Cap của từng khung giờ.
   - Grace Period.
   - Chính sách vé tháng nếu có.
   - Deposit Fee đã thanh toán nếu session đi từ booking.
   - Phí phạt hoặc phụ phí nếu có.
5. Nếu session đi qua nhiều pricing window, hệ thống tách session thành nhiều đoạn thời gian và tính phí riêng cho từng đoạn.
6. Nếu có phần thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính block mới.
7. Nếu thanh toán tiền mặt, hệ thống áp dụng cash rounding rule nếu cần.
8. Nếu thanh toán online, hệ thống giữ nguyên số tiền chính xác.

#### Exception Flow

| Trường hợp                 | Xử lý                                      |
|----------------------------|--------------------------------------------|
| Không tìm thấy session     | Báo không có lượt gửi đang mở.             |
| Thanh toán online thất bại | Cho thanh toán lại hoặc đổi sang tiền mặt. |
| Chưa thanh toán            | Không cho hoàn tất check-out.              |
| Sai biển số                | Staff xử lý ngoại lệ trước khi check-out.  |
| Có phí phát sinh           | Cộng vào tổng phí trước khi thanh toán.    |

---
