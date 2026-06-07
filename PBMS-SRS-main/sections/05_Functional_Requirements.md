# 5. Functional Requirements

## Feature List

| Feature ID | Feature Name                        | Priority | Description                                                                                |
|------------|-------------------------------------|----------|--------------------------------------------------------------------------------------------|
| F-001      | Parking Structure Management        | Must     | Quản lý Building, Floor, Zone, Slot để hệ thống có thể mở rộng.                            |
| F-002      | Driver Account & Vehicle Management | Must     | Cho phép tạo tài khoản Driver không cần có xe ban đầu; một tài khoản có thể thêm nhiều xe. |
| F-003      | Vehicle Check-in                    | Must     | Tạo lượt gửi xe bằng nhập liệu thủ công trên web.                                          |
| F-004      | Parking Allocation                  | Must     | Xe máy và ô tô vãng lai được gợi ý Zone/Area; ô tô booking/thẻ tháng dùng Slot đã chọn/cấp hoặc Slot do hệ thống tự chọn. |
| F-005      | Booking Management                  | Must     | Booking yêu cầu chọn Building trước, nhập biển số, đặt cọc bằng giá block đầu tiên, thời gian đặt trước tối thiểu 1 tiếng từ lúc thanh toán. |
| F-006      | Monthly Card Management             | Must     | Đăng ký/gia hạn thẻ tháng, đảm bảo luôn còn chỗ bằng capacity/slot riêng.                  |
| F-007      | Parking Session Tracking            | Must     | Theo dõi lượt gửi xe từ check-in đến check-out.                                            |
| F-008      | Vehicle Check-out                   | Must     | Tính phí, xác nhận thanh toán và kết thúc lượt gửi xe.                                     |
| F-009      | Payment Management                  | Must     | Hỗ trợ tiền mặt và thanh toán online thật qua ngân hàng; không hỗ trợ thanh toán thẻ.      |
| F-010      | Fee Calculation                     | Must     | Tính phí theo giờ, loại xe, khung giờ, qua đêm và nhiều ngày.                              |
| F-011      | Exception Handling                  | Should   | Xử lý mất mã, sai biển số, quá hạn, gửi sai khu vực, chưa thanh toán.                      |
| F-012      | Operation Monitoring                | Should   | Manager xem trạng thái bãi, doanh thu, lượt xe, tỷ lệ lấp đầy.                             |

---

## FR-001: Parking Structure Management

### Description

Hệ thống cho phép Manager cấu hình cấu trúc bãi xe để phục vụ vận hành và mở rộng sau này.

### Actors

- Parking Manager
- System Administrator

### Preconditions

- Actor đã đăng nhập.
- Actor có quyền quản lý cấu trúc bãi xe.

### Trigger

- Actor mở màn hình quản lý cấu trúc bãi xe.

### Main Flow

1. Actor chọn chức năng quản lý cấu trúc bãi xe.
2. Hệ thống hiển thị danh sách Building, Floor, Zone, Slot hiện có.
3. Actor thêm hoặc cập nhật cấu trúc.
4. Hệ thống kiểm tra dữ liệu hợp lệ.
5. Hệ thống lưu cấu hình.
6. Hệ thống cập nhật dữ liệu để phục vụ phân bổ chỗ đỗ.

### Alternative Flow

- Actor tạm khóa Zone/Slot để bảo trì.
- Actor mở lại Zone/Slot sau khi bảo trì.
- Actor thay đổi Zone dành cho loại xe cụ thể.

### Exception Flow

- Nếu Zone/Slot đang có xe hoặc booking, hệ thống không cho xóa trực tiếp.
- Nếu cấu hình làm vượt hoặc sai sức chứa, hệ thống báo lỗi.
- Nếu actor không có quyền, hệ thống từ chối thao tác.

### Business Rules

- BR-001
- BR-002
- BR-003
- BR-004
- BR-005
- BR-006

### Postconditions

- Hệ thống lưu cấu hình.
- Hệ thống cập nhật dữ liệu để phục vụ phân bổ chỗ đỗ.

### Acceptance Criteria

- Given Manager có quyền quản lý cấu trúc  
  When Manager thêm Building/Floor/Zone/Slot hợp lệ  
  Then hệ thống lưu cấu trúc và cho phép dùng trong phân bổ chỗ.

- Given Zone/Slot đang bảo trì  
  When Staff check-in hoặc Driver booking  
  Then hệ thống không phân bổ Zone/Slot đó.

---

## FR-002: Driver Account & Vehicle Management

### Description

Hệ thống cho phép tạo tài khoản Driver trước, không bắt buộc phải có xe ngay lúc tạo tài khoản. Driver có thể thêm một
hoặc nhiều xe sau này.

### Actors

- Driver
- Parking Staff
- System

### Preconditions

- Driver hoặc Staff có quyền tạo/cập nhật thông tin tài khoản.
- Biển số xe được cung cấp khi thêm xe.

### Trigger

- Driver đăng ký tài khoản.
- Driver thêm xe vào tài khoản.
- Staff tạo hoặc cập nhật tài khoản hộ Driver.

### Main Flow

1. Actor mở màn hình quản lý tài khoản Driver.
2. Actor tạo tài khoản Driver với thông tin cơ bản.
3. Hệ thống cho phép lưu tài khoản dù chưa có xe.
4. Khi cần, Actor chọn Add Vehicle.
5. Actor nhập biển số, loại xe.
6. Hệ thống kiểm tra dữ liệu xe.
7. Hệ thống lưu xe vào tài khoản Driver.

### Alternative Flow

- Staff thêm xe hộ Driver.
- Driver thêm nhiều xe vào cùng một tài khoản.

### Exception Flow

| Trường hợp                                  | Xử lý                                          |
|---------------------------------------------|------------------------------------------------|
| Biển số trống khi thêm xe                   | Hệ thống yêu cầu nhập biển số.                 |
| Loại xe không được hỗ trợ                   | Hệ thống từ chối.                              |
| Biển số đã tồn tại theo rule kiểm tra trùng | Hệ thống cảnh báo hoặc từ chối tùy chính sách. |

### Business Rules

- BR-ACC-001
- BR-ACC-002
- BR-ACC-003
- BR-ACC-004

### Postconditions

- Driver Account được tạo.
- Xe được liên kết với Driver Account nếu Actor thêm xe.

### Acceptance Criteria

- Given Driver chưa có xe  
  When Driver tạo tài khoản  
  Then hệ thống vẫn cho tạo tài khoản thành công.

- Given Driver đã có tài khoản  
  When Driver thêm nhiều xe hợp lệ  
  Then hệ thống lưu các xe vào cùng tài khoản.

---

## FR-003: Vehicle Check-in

### Description

Hệ thống cho phép Staff tạo lượt gửi xe khi xe vào bãi bằng cách nhập dữ liệu thủ công trên web.

### Actors

- Parking Staff
- System

### Preconditions

- Staff đã đăng nhập.
- Bãi xe đang hoạt động.
- Có ít nhất một Zone/Slot còn khả dụng cho loại xe tương ứng.

### Trigger

- Xe đến bãi và cần check-in.

### Main Flow

1. Staff mở màn hình Check-in.
2. Staff nhập biển số hoặc mã booking/mã thẻ tháng.
3. Hệ thống xác định loại xe và trạng thái liên quan:
    - Khách vãng lai.
    - Khách có booking.
    - Khách có thẻ tháng.
4. Hệ thống kiểm tra xe có session đang mở hay không.
5. Hệ thống kiểm tra điều kiện chỗ đỗ:
    - Xe máy: tìm Zone/Area còn sức chứa.
    - Ô tô: tìm Slot còn trống.
    - Khách thẻ tháng: ưu tiên capacity/slot pool dành cho monthly card.
    - Khách booking: kiểm tra booking còn hiệu lực.
6. Hệ thống gợi ý Zone/Slot phù hợp.
7. Staff xác nhận check-in.
8. Hệ thống tạo parking session.
9. Hệ thống cập nhật trạng thái chỗ đỗ.

### Alternative Flow

- Nếu xe có booking hợp lệ, hệ thống lấy thông tin booking để tạo session.
- Nếu xe có thẻ tháng hợp lệ, hệ thống áp dụng quyền lợi thẻ tháng.
- Nếu Staff nhập sai thông tin trước khi xác nhận, Staff có thể sửa lại.

### Exception Flow

| Trường hợp                           | Xử lý                                                   |
|--------------------------------------|---------------------------------------------------------|
| Biển số trống                        | Yêu cầu nhập biển số.                                   |
| Xe đã có session đang mở             | Không cho tạo session mới.                              |
| Booking chưa thanh toán cọc          | Không cho check-in theo booking.                        |
| Booking đã bị hủy do trễ quá 45 phút | Không áp dụng booking, xử lý như khách mới nếu còn chỗ. |
| Hết chỗ phù hợp                      | Từ chối check-in.                                       |
| Zone/Slot bị khóa hoặc bảo trì       | Không phân bổ.                                          |

### Business Rules

- BR-HW-001
- BR-HW-002
- BR-HW-003
- BR-ALLOC-001
- BR-ALLOC-002
- BR-ALLOC-003
- BR-ALLOC-004
- BR-ALLOC-005
- BR-BOOK-008
- BR-MONTH-001
- BR-MONTH-002
- BR-MONTH-003
- BR-028
- BR-030

### Postconditions

- Hệ thống tạo parking session.
- Hệ thống cập nhật trạng thái chỗ đỗ.

### Acceptance Criteria

- Given Staff nhập biển số và loại xe hợp lệ  
  When hệ thống còn chỗ phù hợp  
  Then hệ thống tạo parking session thành công.

- Given xe đang có session chưa hoàn tất  
  When Staff check-in cùng biển số  
  Then hệ thống báo xe đang ở trong bãi.

---

## FR-004: Parking Allocation

### Description

Hệ thống phân bổ chỗ đỗ theo loại xe và theo trạng thái khách vãng lai, booking hoặc thẻ tháng.

### Actors

- Parking Staff
- Driver
- System

### Preconditions

- Cấu trúc Building/Floor/Zone/Slot đã được cấu hình.
- Zone/Slot có trạng thái hợp lệ.
- Loại xe đã được xác định.

### Trigger

- Check-in xe vãng lai.
- Check-in xe có booking.
- Check-in xe có thẻ tháng.

### Main Flow

1. Hệ thống nhận thông tin loại xe và trạng thái khách.
2. Hệ thống xác định nhóm phân bổ:
    - Walk-in.
    - Booking.
    - Monthly Card.
3. Hệ thống kiểm tra Zone/Slot khả dụng.
4. Hệ thống loại bỏ Zone/Slot bảo trì, tạm khóa hoặc không phù hợp.
5. Nếu là xe máy, hệ thống gợi ý Zone/Area còn sức chứa.
6. Nếu là ô tô vãng lai, hệ thống gợi ý Zone còn sức chứa, không bắt buộc chọn Slot cụ thể.
7. Nếu là ô tô booking, hệ thống dùng Slot đã chọn hoặc Slot do hệ thống tự chọn khi xác nhận booking.
8. Nếu là ô tô thẻ tháng, hệ thống dùng Slot riêng đã cấp cho thẻ tháng.
9. Nếu là khách thẻ tháng, hệ thống ưu tiên phần capacity/slot pool đã giữ cho nhóm thẻ tháng.
10. Staff xác nhận phân bổ.
11. Hệ thống cập nhật trạng thái.

### Alternative Flow

- Staff có thể chọn Zone/Slot khác nếu vẫn hợp lệ.
- Hệ thống có thể gợi ý nhiều lựa chọn nếu có nhiều chỗ phù hợp.

### Exception Flow

| Trường hợp                              | Xử lý                          |
|-----------------------------------------|--------------------------------|
| Không còn Zone cho xe máy               | Từ chối check-in.              |
| Không còn Zone/Slot phù hợp cho ô tô    | Từ chối check-in.              |
| Phần chỗ còn lại phải giữ cho thẻ tháng | Không cấp cho walk-in/booking. |
| Zone/Slot bảo trì                       | Không phân bổ.                 |

### Business Rules

- BR-ALLOC-001
- BR-ALLOC-002
- BR-ALLOC-003
- BR-ALLOC-004
- BR-ALLOC-005

### Postconditions

- Xe được gán Zone/Slot phù hợp.
- Capacity/Slot status được cập nhật.

### Acceptance Criteria

- Given xe máy check-in  
  When còn Zone phù hợp  
  Then hệ thống gợi ý Zone/Area còn sức chứa.

- Given ô tô check-in  
  When còn Slot phù hợp  
  Then hệ thống gán Slot cụ thể.

- Given khách thẻ tháng check-in  
  When còn capacity/slot pool dành cho monthly card  
  Then hệ thống ưu tiên phân bổ từ phần đã giữ cho thẻ tháng.

---

## FR-005: Booking Management

### Description

Hệ thống cho phép Driver đặt trước chỗ gửi xe.
Booking yêu cầu chọn Building trước, nhập biển số, thời gian gửi dự kiến và thanh toán Deposit Fee.
Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
Booking phải được tạo tối thiểu 1 tiếng và tối đa 8 tiếng trước thời điểm vào bãi tính từ lúc thanh toán cọc thành công.
Xe máy có thể chọn Zone hoặc để hệ thống tự chọn Zone phù hợp. Ô tô có thể chọn Slot cụ thể hoặc để hệ thống tự chọn Zone/Slot phù hợp trong Building đã chọn.

### Actors

- Driver
- Parking Staff
- System
- Bank Payment Gateway

### Preconditions

- Driver có tài khoản hoặc được Staff tạo booking hộ.
- Driver chọn Building trước khi tạo booking.
- Driver cung cấp biển số xe.
- Driver cung cấp thời gian dự kiến vào bãi.
- Driver cung cấp thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
- Hệ thống còn chỗ phù hợp trong Building đã chọn:
    - Xe máy: còn capacity theo Zone.
    - Ô tô: còn Zone/Slot phù hợp; Slot có thể do Driver chọn hoặc do hệ thống tự chọn.
- Người dùng thanh toán cọc thành công.

### Trigger

- Driver muốn đặt trước chỗ gửi xe.

### Main Flow

1. Driver mở chức năng Booking.
2. Driver chọn Building muốn gửi xe.
3. Driver chọn hoặc nhập biển số xe.
4. Driver chọn loại xe.
5. Driver nhập thời gian dự kiến vào bãi.
6. Driver nhập thời lượng gửi xe hoặc thời gian dự kiến rời bãi.
7. Hệ thống kiểm tra thời gian đặt trước:
    - Không nhỏ hơn 1 tiếng.
    - Không lớn hơn 8 tiếng.
8. Hệ thống xác định bảng giá hiện hành theo loại xe và thời điểm booking.
9. Hệ thống tính Deposit Fee bằng giá của block đầu tiên theo bảng giá hiện hành.
10. Hệ thống kiểm tra chỗ phù hợp trong Building đã chọn:
    - Xe máy: Driver có thể chọn Zone; nếu không chọn, hệ thống tự chọn Zone phù hợp còn capacity.
    - Ô tô: Driver có thể chọn Slot cụ thể; nếu không chọn Slot/Zone, hệ thống tự chọn Zone/Slot phù hợp còn khả dụng.
11. Hệ thống tạo yêu cầu đặt cọc.
12. Driver thanh toán cọc qua ngân hàng.
13. Bank Payment Gateway trả kết quả thanh toán.
14. Nếu thanh toán thành công, hệ thống xác nhận booking.
15. Khi Driver đến bãi trong thời gian hợp lệ, hệ thống chuyển booking thành parking session.
16. Nếu booking giữ Zone/Slot cụ thể, hệ thống giữ Zone capacity hoặc chuyển Slot sang RESERVED sau khi thanh toán cọc thành công.
17. Nếu khách không thanh toán trong thời gian booking payment timeout, hệ thống tự động hủy booking.

### Alternative Flow

- Staff tạo booking hộ Driver.
- Driver hủy booking trước thời gian quy định nếu chính sách cho phép.
- Driver đổi thời gian booking nếu còn capacity/Zone/Slot phù hợp trong Building đã chọn.

### Exception Flow

| Trường hợp                                                     | Xử lý                                       |
|----------------------------------------------------------------|---------------------------------------------|
| Không chọn Building                                            | Không cho tạo booking.                      |
| Không nhập biển số                                             | Không cho tạo booking.                      |
| Thời gian booking không đủ tối thiểu 1 tiếng từ lúc thanh toán | Từ chối booking.                            |
| Thanh toán cọc thất bại                                        | Booking không được confirmed.               |
| Không còn chỗ phù hợp                                          | Từ chối booking.                            |
| Đến trễ quá 45 phút                                            | Hủy booking/parking reservation và mất cọc. |
| Booking trùng biển số trong cùng thời gian                     | Cảnh báo hoặc từ chối.                      |

### Business Rules

- BR-BOOK-001
- BR-BOOK-002
- BR-BOOK-003
- BR-BOOK-004
- BR-BOOK-005
- BR-BOOK-006
- BR-BOOK-007
- BR-BOOK-008
- BR-BOOK-009
- BR-BOOK-010

### Postconditions

- Booking được tạo ở trạng thái Confirmed nếu cọc thành công.
- Capacity/Zone được giữ theo rule.
- Booking có thể chuyển thành Parking Session khi check-in hợp lệ.

### Acceptance Criteria

- Given Driver nhập biển số, thời gian vào bãi và thời lượng gửi xe hợp lệ  
  When Driver thanh toán Deposit Fee bằng giá block đầu tiên thành công  
  Then hệ thống tạo booking confirmed.

- Given booking giữ slot cụ thể  
  When thanh toán Deposit Fee thành công  
  Then Slot chuyển sang RESERVED.

- Given Driver chưa thanh toán booking sau booking payment timeout  
  When hệ thống kiểm tra trạng thái booking  
  Then booking bị hủy và slot/capacity được trả về available.

- Given Driver không check-in trong check-in grace time  
  When hệ thống kiểm tra booking  
  Then booking bị hủy và Deposit Fee không được hoàn trả.

- Given Driver đặt booking dưới 1 tiếng từ thời điểm thanh toán  
  When Driver xác nhận booking  
  Then hệ thống từ chối.

- Given Driver đặt booking quá 8 tiếng từ thời điểm thanh toán  
  When Driver xác nhận booking  
  Then hệ thống từ chối.

- Given Driver booking xe máy  
  When còn capacity phù hợp  
  Then hệ thống xác nhận booking nhưng không gán Zone/Slot cụ thể.

- Given Driver booking ô tô  
  When còn Slot phù hợp  
  Then hệ thống cho Driver chọn Slot và giữ Slot đó sau khi cọc thành công.

- Given Driver hủy booking trước giờ booking ít nhất 1 tiếng  
  When hệ thống xử lý hủy  
  Then booking bị hủy và cọc được hoàn.

- Given Driver đến trễ quá 45 phút  
  When hệ thống kiểm tra booking  
  Then booking bị hủy và cọc không hoàn lại.

- Given Driver đến sớm hơn giờ booking  
  When còn chỗ phù hợp  
  Then hệ thống cho check-in sớm và bắt đầu tính giờ từ thời điểm check-in thực tế.

---

## FR-006: Monthly Card Management

### Description

Hệ thống cho phép Driver đăng ký/gia hạn thẻ tháng. Thẻ tháng xe máy đảm bảo có chỗ nhưng không phân theo Zone/Slot cụ
thể. Thẻ tháng ô tô được cấp Slot riêng trong thời hạn thẻ.
Vé tháng áp dụng cho biển số đăng ký cố định và được thanh toán trả trước theo chu kỳ.
Vé tháng có thời hạn hiệu lực, có thể cấu hình quyền gửi qua đêm và mỗi thẻ chỉ áp dụng cho một xe đã đăng ký.

### Actors

- Driver
- Parking Staff
- Parking Manager
- System
- Bank Payment Gateway

### Preconditions

- Driver có tài khoản.
- Driver đã có hoặc nhập thông tin xe.
- Hệ thống còn quota/capacity đảm bảo cho thẻ tháng.
- Thanh toán phí thẻ tháng thành công.

### Trigger

- Driver muốn đăng ký hoặc gia hạn thẻ tháng.

### Main Flow

1. Driver hoặc Staff mở chức năng Monthly Card.
2. Driver chọn hoặc thêm xe.
3. Hệ thống kiểm tra biển số và loại xe.
4. Hệ thống kiểm tra xe có thẻ tháng còn hiệu lực hay không.
5. Hệ thống kiểm tra khả năng đảm bảo chỗ:
    - Xe máy: kiểm tra capacity dành cho nhóm thẻ tháng xe máy.
    - Ô tô: kiểm tra Slot còn có thể cấp riêng cho thẻ tháng ô tô.
6. Nếu còn khả năng đảm bảo, hệ thống tạo yêu cầu thanh toán.
7. Driver thanh toán phí thẻ tháng.
8. Hệ thống nhận kết quả thanh toán.
9. Nếu thanh toán thành công, hệ thống kích hoạt thẻ tháng.
10. Khi xe vào bãi:
- Xe máy thẻ tháng được gợi ý Zone còn chỗ.
- Ô tô thẻ tháng được điều hướng tới Slot riêng đã cấp.
11. Hệ thống lưu thời hạn hiệu lực của vé tháng.
12. Hệ thống lưu cấu hình allow_overnight nếu áp dụng.
13. Hệ thống xác nhận mỗi vé tháng chỉ liên kết với một xe đã đăng ký.
14. Hệ thống hỗ trợ gia hạn vé tháng; sau khi gia hạn thành công, quyền lợi vé tháng được kích hoạt lại ngay lập tức.

### Alternative Flow

- Staff đăng ký thẻ tháng hộ Driver.
- Driver gia hạn thẻ tháng trước khi hết hạn.
- Manager điều chỉnh quota thẻ tháng nếu chính sách vận hành thay đổi.
- Nếu vé tháng hết hạn và xe vẫn còn trong bãi, hệ thống downgrade quyền lợi vé tháng và tính phí vãng lai từ thời điểm hết hạn.

### Exception Flow

| Trường hợp                      | Xử lý                                 |
|---------------------------------|---------------------------------------|
| Xe chưa được thêm               | Yêu cầu thêm xe hoặc nhập biển số.    |
| Xe đã có thẻ tháng còn hiệu lực | Không cho tạo trùng.                  |
| Hết quota đảm bảo chỗ           | Từ chối đăng ký/gia hạn.              |
| Thanh toán thất bại             | Không kích hoạt thẻ tháng.            |
| Thẻ tháng hết hạn               | Không áp dụng quyền lợi khi check-in. |

### Business Rules

- BR-MONTH-001
- BR-MONTH-002
- BR-MONTH-003
- BR-MONTH-004
- BR-MONTH-005
- BR-MONTH-006
- BR-MONTH-007

### Postconditions

- Thẻ tháng được kích hoạt nếu thanh toán thành công.
- Capacity/slot pool dành cho monthly card được cập nhật.
- Driver có thể gửi xe nhiều lượt/ngày trong thời gian thẻ còn hiệu lực.

### Acceptance Criteria

- Given Driver đăng ký thẻ tháng xe máy  
  When còn capacity đảm bảo  
  Then thẻ tháng được kích hoạt sau khi thanh toán thành công.

- Given Driver đăng ký thẻ tháng ô tô  
  When còn Slot phù hợp  
  Then hệ thống cấp Slot riêng cho xe sau khi thanh toán thành công.

- Given không còn Slot dành cho ô tô thẻ tháng  
  When Driver đăng ký thẻ tháng ô tô  
  Then hệ thống từ chối và báo hết slot thẻ tháng ô tô.

- Given vé tháng hết hạn và xe vẫn còn trong bãi  
  When hệ thống quét trạng thái lúc 00:00  
  Then hệ thống downgrade vé tháng và bắt đầu tính phí vãng lai.

- Given khách gia hạn vé tháng thành công  
  When thanh toán gia hạn được xác nhận  
  Then quyền lợi vé tháng được kích hoạt lại ngay lập tức.

- Given vé tháng giới hạn số lượng biển số  
  When Driver thêm biển số vượt giới hạn  
  Then hệ thống từ chối thêm biển số.

---

## FR-007: Parking Session Tracking

### Description

Hệ thống theo dõi một lượt gửi xe từ lúc check-in đến lúc check-out.

### Actors

- Parking Staff
- Driver
- Parking Manager
- System

### Preconditions

- Parking session đã được tạo.

### Trigger

- Staff, Driver hoặc Manager cần xem trạng thái lượt gửi.

### Main Flow

1. Actor mở màn hình theo dõi lượt gửi.
2. Hệ thống hiển thị danh sách session.
3. Actor tìm theo biển số, mã gửi xe, mã booking hoặc trạng thái.
4. Hệ thống hiển thị thông tin:
    - Biển số.
    - Loại xe.
    - Giờ vào.
    - Zone/Slot.
    - Trạng thái.
    - Phí tạm tính nếu có.
5. Actor xem chi tiết session.

### Alternative Flow

- Driver chỉ xem session của chính mình.
- Manager xem toàn bộ session.
- Staff lọc session theo cổng/khu vực/trạng thái.

### Exception Flow

- Không tìm thấy session.
- Session đã check-out.
- Actor không có quyền xem session.

### Business Rules

- BR-028
- BR-029
- BR-030
- BR-031

### Postconditions

- Actor xem được thông tin session theo quyền.

### Acceptance Criteria

- Given xe đã check-in  
  When Staff tìm bằng biển số  
  Then hệ thống hiển thị session đang mở.

- Given Driver có session đang gửi  
  When Driver mở màn hình theo dõi  
  Then hệ thống hiển thị thông tin lượt gửi hiện tại.

---

## FR-008: Vehicle Check-out

### Description

Hệ thống cho phép Staff xử lý xe ra bãi, tính phí, thanh toán và cập nhật trạng thái chỗ đỗ.

### Actors

- Parking Staff
- System
- Driver

### Preconditions

- Xe có parking session đang mở.
- Staff có quyền check-out.

### Trigger

- Driver muốn rời bãi.

### Main Flow

1. Staff mở màn hình Check-out.
2. Staff nhập biển số hoặc mã gửi xe.
3. Hệ thống tìm parking session đang mở.
4. Hệ thống tính phí dựa trên:
    - Loại xe.
    - Thời gian gửi.
    - Khung giờ sáng/tối.
    - Qua đêm.
    - Nhiều ngày.
    - Chính sách thẻ tháng nếu có.
5. Driver chọn phương thức thanh toán:
    - Tiền mặt.
    - Online qua ngân hàng.
6. Hệ thống xác nhận trạng thái thanh toán:
    - Tiền mặt: Staff xác nhận.
    - Online: ngân hàng/payment gateway xác nhận.
7. Khi thanh toán thành công, hệ thống kết thúc session.
8. Hệ thống giải phóng Zone/Slot.

### Alternative Flow

- Nếu Driver có thẻ tháng hợp lệ, hệ thống áp dụng chính sách thẻ tháng.
- Nếu Driver đã thanh toán online trước, hệ thống kiểm tra trạng thái thanh toán.
- Nếu có phí phát sinh, hệ thống cộng vào tổng phí.

### Exception Flow

| Trường hợp                 | Xử lý                                       |
|----------------------------|---------------------------------------------|
| Không tìm thấy session     | Báo không có lượt gửi đang mở.              |
| Thanh toán online thất bại | Cho thanh toán lại hoặc đổi sang tiền mặt.  |
| Chưa thanh toán            | Không cho hoàn tất check-out.               |
| Sai biển số                | Staff xử lý ngoại lệ trước khi check-out.   |
| Có phí phát sinh           | Cộng vào tổng phí trước khi thanh toán.     |
| Mất mã gửi xe              | Staff xác minh bằng biển số/thông tin khác. |

### Business Rules

- BR-032
- BR-033
- BR-034
- BR-035
- BR-036
- BR-PAY-006
- BR-FEE-001
- BR-FEE-002
- BR-FEE-003
- BR-FEE-004
- BR-FEE-005
- BR-FEE-007

### Postconditions

- Hệ thống kết thúc session.
- Hệ thống giải phóng Zone/Slot.

### Acceptance Criteria

- Given xe có session đang mở  
  When Staff xác nhận check-out và thanh toán thành công  
  Then session kết thúc và chỗ đỗ được giải phóng.

---

## FR-009: Payment Management

### Description

Hệ thống hỗ trợ thanh toán tiền mặt và thanh toán online thật qua ngân hàng. Hệ thống không hỗ trợ thanh toán bằng thẻ.

### Actors

- Driver
- Parking Staff
- System
- Bank Payment Gateway

### Preconditions

- Có khoản phí cần thanh toán.
- Khoản phí thuộc một trong các nghiệp vụ: parking fee, booking deposit, monthly card fee hoặc phí phát sinh.

### Trigger

- Driver thanh toán phí.

### Main Flow - Cash Payment

1. Staff chọn phương thức tiền mặt.
2. Hệ thống hiển thị số tiền cần thu.
3. Staff nhận tiền từ Driver.
4. Staff xác nhận đã thu tiền.
5. Hệ thống cập nhật trạng thái Paid.
6. Nếu số tiền cần thanh toán có số lẻ, discount hoặc VAT, hệ thống áp dụng cash rounding rule.
7. Hệ thống hiển thị số tiền sau làm tròn để Staff thu tiền.

### Main Flow - Bank Online Payment

1. Driver chọn thanh toán online qua ngân hàng.
2. Hệ thống tạo yêu cầu thanh toán.
3. Driver thực hiện thanh toán.
4. Bank Payment Gateway trả kết quả.
5. Nếu thành công, hệ thống cập nhật trạng thái Paid.
6. Nếu thất bại, hệ thống giữ trạng thái Pending.

### Alternative Flow

- Cho thanh toán lại hoặc đổi sang tiền mặt nếu nghiệp vụ cho phép.

### Exception Flow

| Trường hợp                   | Xử lý                                                             |
|------------------------------|-------------------------------------------------------------------|
| Thanh toán online thất bại   | Cho thanh toán lại hoặc đổi sang tiền mặt nếu nghiệp vụ cho phép. |
| Thanh toán chưa xác nhận     | Không hoàn tất check-out/booking/monthly card.                    |
| Người dùng chọn card payment | Không hiển thị hoặc báo không hỗ trợ.                             |
| Staff xác nhận sai           | Cần xử lý điều chỉnh theo quyền quản lý.                          |
| Booking quá thời gian thanh toán | Hệ thống tự động hủy booking và trả slot/capacity về available. |
| Cần hoàn cọc booking | Hệ thống tạo trạng thái refund/refunded theo chính sách hoàn tiền. |

### Business Rules

- BR-PAY-001
- BR-PAY-002
- BR-PAY-003
- BR-PAY-004
- BR-PAY-005
- BR-PAY-006
- BR-PAY-007

### Postconditions

- Khoản phí được cập nhật trạng thái thanh toán.
- Nghiệp vụ liên quan chỉ tiếp tục khi payment hợp lệ.

### Acceptance Criteria

- Given Driver chọn online banking  
  When ngân hàng xác nhận thành công  
  Then hệ thống cập nhật payment là Paid.

- Given Driver chọn thanh toán bằng thẻ  
  When hệ thống hiển thị phương thức thanh toán  
  Then card payment không xuất hiện.

- Given khách thanh toán tiền mặt và số tiền cần làm tròn  
  When hệ thống áp dụng cash rounding rule  
  Then số tiền thu được làm tròn theo cash_rounding_unit và rounding_threshold.

- Given khách thanh toán online  
  When hệ thống tạo giao dịch  
  Then số tiền thanh toán giữ nguyên giá trị chính xác, không làm tròn.

---

## FR-010: Fee Calculation

### Description

Hệ thống tính phí gửi xe dựa trên thời gian thực tế sử dụng, loại xe, pricing window, base duration, base price, increment block, increment price, window cap, grace period và các phụ phí/phí phạt nếu có.

### Actors

- Parking Staff
- Driver
- System

### Preconditions

- Parking session có thời gian check-in.
- Có bảng giá hợp lệ theo loại xe và pricing window.
- Các biến cấu hình tính phí đã được thiết lập.

### Trigger

- Staff thực hiện check-out.
- Driver xem phí tạm tính.
- Hệ thống cần tính tổng phí.

### Main Flow

1. Hệ thống lấy thông tin parking session.
2. Hệ thống xác định loại xe.
3. Hệ thống xác định thời gian check-in và check-out.
4. Hệ thống xác định các pricing window mà session đi qua.
5. Nếu session đi qua nhiều pricing window, hệ thống tách session thành các đoạn thời gian tương ứng.
6. Với từng đoạn thời gian, hệ thống áp dụng:
   - Base Duration.
   - Base Price.
   - Increment Block.
   - Increment Price.
   - Window Cap.
7. Nếu thời gian phát sinh nhỏ hơn hoặc bằng grace period, hệ thống không tính thêm block mới.
8. Nếu thời gian phát sinh lớn hơn grace period, hệ thống tính thành block mới.
9. Nếu session có booking deposit, hệ thống trừ deposit khỏi số tiền cần thanh toán theo chính sách booking.
10. Nếu session có phụ phí hoặc phí phạt, hệ thống cộng vào tổng phí.
11. Nếu thanh toán tiền mặt, hệ thống áp dụng cash rounding rule.
12. Nếu thanh toán online, hệ thống giữ nguyên giá trị chính xác.
13. Hệ thống hiển thị tổng phí cần thanh toán.

### Alternative Flow

- Nếu xe có vé tháng hợp lệ, hệ thống áp dụng chính sách vé tháng.
- Nếu vé tháng hết hạn trong lúc xe vẫn còn trong bãi, hệ thống tính phí vãng lai từ thời điểm hết hạn.
- Nếu xe check-out trễ hơn thời gian booking, phần phát sinh được tính theo block pricing của bảng giá vãng lai.
- Nếu có lost card penalty hoặc wrong zone penalty, hệ thống cộng vào tổng phí.

### Exception Flow

| Trường hợp | Xử lý |
|---|---|
| Thiếu bảng giá | Hệ thống báo không thể tính phí. |
| Thiếu pricing window | Hệ thống báo thiếu cấu hình khung giờ. |
| Thiếu base duration/base price/increment block/increment price | Hệ thống báo thiếu cấu hình bảng giá. |
| Thời gian check-out nhỏ hơn check-in | Hệ thống báo lỗi dữ liệu. |
| Không xác định được loại xe | Hệ thống yêu cầu cập nhật thông tin. |
| Thiếu cấu hình grace period hoặc rounding | Hệ thống báo thiếu cấu hình chính sách tính phí. |

### Business Rules

- BR-FEE-001
- BR-FEE-002
- BR-FEE-003
- BR-FEE-004
- BR-FEE-005
- BR-FEE-006
- BR-FEE-007
- BR-FEE-008
- BR-FEE-009
- BR-FEE-010
- BR-FEE-011
- BR-FEE-012
- BR-FEE-013
- BR-FEE-014
- BR-FEE-015

### Postconditions

- Tổng phí được tính và hiển thị.
- Payment record có thể được tạo dựa trên tổng phí.
- Nếu có rounding, hệ thống lưu cả số tiền gốc và số tiền sau làm tròn nếu cần đối soát.

### Acceptance Criteria

- Given xe có session hợp lệ  
  When Staff check-out  
  Then hệ thống tính phí theo loại xe và thời gian thực tế sử dụng.

- Given session đi qua nhiều pricing window  
  When hệ thống tính phí  
  Then hệ thống tách session theo từng pricing window và áp dụng bảng giá riêng cho từng window.

- Given phí trong một pricing window vượt window cap  
  When hệ thống tính phí window đó  
  Then phí của window đó không vượt quá window cap.

- Given session đi qua ngày mới  
  When hệ thống tính phí  
  Then session không bị reset và tiếp tục được tính theo pricing window tương ứng.

- Given thời gian phát sinh nhỏ hơn hoặc bằng grace period  
  When hệ thống tính block phát sinh  
  Then hệ thống không tính block mới.

- Given thời gian phát sinh lớn hơn grace period  
  When hệ thống tính block phát sinh  
  Then hệ thống tính thành block mới.

- Given khách thanh toán tiền mặt và số tiền cần làm tròn  
  When hệ thống tính tổng phí  
  Then hệ thống áp dụng cash rounding rule.

- Given khách thanh toán online  
  When hệ thống tính tổng phí  
  Then hệ thống giữ nguyên số tiền chính xác, không làm tròn.

---

## FR-011: Exception Handling

### Description

Hệ thống hỗ trợ Staff xử lý các tình huống ngoại lệ trong vận hành.

### Actors

- Parking Staff
- Parking Manager
- System

### Preconditions

- Có tình huống ngoại lệ phát sinh.
- Staff có quyền xử lý.

### Trigger

- Có tình huống ngoại lệ phát sinh.

### Supported Exceptions

| Exception                  | Mô tả                                        |
|----------------------------|----------------------------------------------|
| Lost Virtual Card Code | Driver mất mã gửi xe/mã thẻ mô phỏng. |
| Wrong Plate Number | Biển số nhập sai hoặc không khớp. |
| Overdue Parking | Xe gửi quá thời gian dự kiến/booking. |
| Wrong Zone Parking | Xe đỗ sai khu vực quy định hoặc quá thời gian cho phép ở khu vực đó. |
| Unpaid Session | Xe chưa thanh toán. |
| Slot Occupied Unexpectedly | Slot đã bị chiếm hoặc trạng thái không đúng. |
| Lost Card Penalty | Phí phạt áp dụng khi Staff chuyển trạng thái vé thành LOST. |
| Wrong Zone Penalty | Phí phạt áp dụng khi xe đỗ sai khu vực và được Staff xác nhận. |

### Main Flow

1. Staff mở session hoặc booking có vấn đề.
2. Staff chọn loại ngoại lệ.
3. Hệ thống hiển thị thông tin liên quan.
4. Staff nhập lý do xử lý.
5. Hệ thống yêu cầu xác nhận.
6. Hệ thống cập nhật trạng thái hoặc phí phát sinh nếu có.
7. Hệ thống lưu kết quả xử lý.
8. Nếu ngoại lệ phát sinh phí phạt, hệ thống cộng phí phạt vào tổng phí cần thanh toán.
9. Nếu là mất vé/mã gửi xe, hệ thống chỉ cho phép xe rời bãi sau khi khách thanh toán phí gửi xe hiện tại và lost card penalty.

### Alternative Flow

- Một số ngoại lệ cần Manager xác nhận.

### Exception Flow

- Xe chưa thanh toán không được hoàn tất check-out, trừ khi Manager xử lý đặc biệt.
- Sai biển số phải được chỉnh trước khi hoàn tất session.

### Business Rules

- BR-042
- BR-043
- BR-044
- BR-045
- BR-046

### Postconditions

- Hệ thống cập nhật trạng thái hoặc phí phát sinh nếu có.
- Hệ thống lưu kết quả xử lý.

### Acceptance Criteria

- Given session có lỗi sai biển số  
  When Staff cập nhật và xác nhận lý do  
  Then hệ thống lưu thông tin đã chỉnh sửa.

- Given xe chưa thanh toán  
  When Staff cố check-out  
  Then hệ thống chặn hoàn tất check-out.

- Given Staff chuyển trạng thái vé thành LOST  
  When hệ thống tính phí check-out  
  Then hệ thống cộng phí gửi xe hiện tại và lost card penalty vào tổng phí.

- Given xe bị xác nhận đỗ sai khu vực  
  When Staff xử lý ngoại lệ  
  Then hệ thống cộng wrong zone penalty nếu chính sách áp dụng.

---

## FR-012: Operation Monitoring

### Description

Hệ thống cho phép Manager theo dõi tình trạng vận hành của bãi xe.

### Actors

- Parking Manager
- System

### Preconditions

- Manager đã đăng nhập.
- Có dữ liệu vận hành.

### Trigger

- Manager mở dashboard vận hành.

### Main Flow

1. Manager mở dashboard vận hành.
2. Hệ thống hiển thị:
    - Tổng số Zone/Slot.
    - Số chỗ còn trống.
    - Số chỗ đang sử dụng.
    - Số chỗ đã reserved.
    - Số chỗ bảo trì/tạm khóa.
    - Lượt xe vào/ra.
    - Doanh thu.
    - Tỷ lệ lấp đầy.
3. Manager lọc theo Building, Floor, Zone, loại xe hoặc thời gian.
4. Hệ thống cập nhật kết quả hiển thị.

### Alternative Flow

- Manager lọc theo Building, Floor, Zone, loại xe hoặc thời gian.

### Exception Flow

- Không có dữ liệu vận hành.

### Business Rules

- BR-047
- BR-048
- BR-049
- BR-050
- BR-051

### Postconditions

- Hệ thống hiển thị tình trạng bãi xe và doanh thu tương ứng.

### Acceptance Criteria

- Given Manager mở dashboard  
  When hệ thống có dữ liệu session/payment  
  Then hệ thống hiển thị tình trạng bãi xe và doanh thu tương ứng.

---
