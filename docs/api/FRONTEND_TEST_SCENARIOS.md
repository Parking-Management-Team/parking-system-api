# 🧪 Kịch Bản Kiểm Thử Giao Diện (Frontend Test Scenarios)

Tài liệu này cung cấp các kịch bản kiểm thử (Test Scenarios) thực tế sử dụng chính xác dữ liệu mẫu được nạp sẵn từ hệ thống (Seed Data) để hỗ trợ đội ngũ Front-End (FE) và QA kiểm thử giao diện một cách chuẩn xác.

---

## 🔑 1. Thông Tin Dữ Liệu Kiểm Thử Có Sẵn (Seed Data)

### A. Tài khoản kiểm thử (Credentials)
* **Khách hàng (Driver):** `driver` / Mật khẩu: `Password123` (Email: `driver@pbms.com`)
* **Nhân viên (Staff):** `staff` / Mật khẩu: `Password123` (Email: `staff@pbms.com`)
* **Quản lý (Manager):** `manager` / Mật khẩu: `Password123` (Email: `manager@pbms.com`)
* **Quản trị (Admin):** `admin` / Mật khẩu: `Password123` (Email: `admin@pbms.com`)

### B. Cấu trúc bãi đỗ xe và Phương tiện
* **Tòa nhà:** `Building A` (Mã: `BLD01`)
* **Phân khu & Vị trí:**
  * Tầng 1: Phân khu xe máy `ZM01` (Motorbike Zone) - Sức chứa 100 xe.
  * Tầng 2: Phân khu ô tô `ZC01` (Car Zone) - Gồm 10 Slots ký hiệu từ `ZC01-01` đến `ZC01-10`.
* **Thẻ RFID vật lý:** `CARD001`, `CARD002`, `CARD003`.
* **Xe của Driver:** Biển số `51G-12345` (Loại xe: Ô tô/Car, trạng thái: ACTIVE).

### C. Thông tin thẻ Test cổng VNPay (Sandbox)
* **Số thẻ:** `9704198526191432185`
* **Tên chủ thẻ:** `NGUYEN VAN A`
* **Ngày phát hành:** `07/15`
* **Mã OTP:** `123456`

---

## 🚗 2. Role: Customer (Tài khoản `driver`)

### Kịch bản 1.1: Đặt chỗ trước thành công (Booking Flow)
* **Mục tiêu:** Khách đặt chỗ trước cho xe `51G-12345` tại `Building A` và thanh toán cọc thành công.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `driver`.
  2. Tạo đơn đặt chỗ:
     * Chọn Tòa nhà: **Building A**
     * Chọn xe: **51G-12345** (Ô tô)
     * Chọn thời gian bắt đầu và kết thúc (Chọn 2 tiếng trong tương lai).
  3. Bấm **Đặt Chỗ** -> Hệ thống tạo đơn hàng trạng thái `PENDING`.
  4. Bấm **Thanh toán tiền cọc** -> Redirect sang cổng VNPay Sandbox.
  5. Nhập thông tin thẻ Test VNPay và mã OTP `123456`.
  6. Sau khi thanh toán thành công, hệ thống tự động redirect về trang lịch sử đặt chỗ.
* **Kết quả mong đợi:** Đơn đặt chỗ chuyển sang trạng thái **`CONFIRMED`** (Màu xanh lá) và hiển thị mã QR check-in của khách.

### Kịch bản 1.2: Đăng ký & Gia hạn vé tháng (Subscription Flow)
* **Mục tiêu:** Mua gói vé tháng cho xe `51G-12345` và gia hạn vé.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `driver`.
  2. Chọn **Đăng ký vé tháng** -> Chọn xe **51G-12345** -> Chọn tòa nhà **Building A**.
  3. Màn hình mua hiển thị giá vé (mặc định là `1.000.000đ` cho ô tô) và thời hạn `30 ngày` (hoặc `90 ngày` nếu đã cập nhật cấu hình).
  4. Bấm **Đăng ký & Thanh toán** -> Trải qua luồng thanh toán VNPay Sandbox.
  5. Sau khi thanh toán thành công, truy cập trang chi tiết vé tháng -> Bấm **Gia hạn (Renew)** -> Thanh toán tiếp.
* **Kết quả mong đợi:** 
  * Sau bước 4: Vé tháng hiển thị trạng thái **`ACTIVE`** có thời hạn sử dụng 30 ngày.
  * Sau bước 5: Thời hạn hết hạn (`ExpiredAt`) tự động cộng thêm đúng số ngày hiệu lực của gói cấu hình (ví dụ: tăng từ 30 ngày lên 60 ngày).

### Kịch bản 1.3: Đăng nhập qua Google (Google Login OAuth Flow)
* **Mục tiêu:** Khách hàng sử dụng tài khoản Google để đăng nhập nhanh vào hệ thống FE.
* **Các bước thực hiện:**
  1. FE tích hợp nút "Đăng nhập bằng Google" sử dụng thư viện Google Identity Services SDK.
  2. Khách hàng click vào nút và đăng nhập tài khoản Google cá nhân.
  3. Google trả về `Credential Token` (ID Token).
  4. FE gửi Token này qua API `POST /api/auth/google`.
* **Kết quả mong đợi:** API phản hồi thành công trả về JWT Token và thông tin tài khoản tương ứng. FE lưu JWT vào LocalStorage để xác thực cho các request tiếp theo.

---

## 🧑‍✈️ 3. Role: Staff (Tài khoản `staff`)

### Kịch bản 2.1: Quẹt thẻ vào/ra bãi (Gate Session Simulation)
* **Mục tiêu:** Thực hiện Check-in / Check-out xe gửi lượt tại cổng.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `staff`.
  2. **Mô phỏng Check-In (Xe vào):** 
     * Nhập mã thẻ: `CARD002` (Trạng thái đang trống).
     * Nhập biển số xe: `51A-99999` (Xe lượt vãng lai).
     * Bấm **Xác nhận Vào**.
  3. **Mô phỏng Check-Out (Xe ra):**
     * Nhập mã thẻ: `CARD002` -> Bấm **Tính phí & Cho ra**.
* **Kết quả mong đợi:**
  * Khi Check-In: Giao diện hiển thị Check-In thành công và hiển thị vị trí đỗ được chỉ định.
  * Khi Check-Out: Giao diện hiển thị tổng số tiền phí phải thu dựa trên biểu giá gửi xe lượt của bãi. Staff xác nhận đã thu tiền mặt và bấm hoàn thành phiên gửi xe.

### Kịch bản 2.2: Báo mất thẻ và cấp thẻ thay thế (Lost Card Replacement)
* **Mục tiêu:** Xử lý sự cố khi khách gửi xe lượt bị mất thẻ `CARD001`.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `staff`.
  2. Trên hệ thống có sẵn 1 phiên đỗ **ACTIVE** của xe `51G-12345` đang đỗ tại vị trí `ZC01-01` bằng thẻ `CARD001` (dữ liệu nạp sẵn).
  3. Vào danh sách xe đang gửi -> Chọn xe `51G-12345` -> Chọn **Báo Mất & Đổi Thẻ**.
  4. Nhập mã thẻ RFID mới thay thế: `CARD003`. Bấm **Xác nhận**.
* **Kết quả mong đợi:** 
  * Thẻ `CARD001` tự động chuyển sang trạng thái bị khóa/mất.
  * Hệ thống tự động tạo sự cố `LOST_CARD` kèm hóa đơn phạt tiền mất thẻ. 
  * Phiên đỗ xe được chuyển sang liên kết với thẻ `CARD003`. Khi khách ra, quét thẻ `CARD003` để thanh toán phí phạt và cho xe ra bình thường.

### Kịch bản 2.3: Báo cáo sự cố hư hại phương tiện tại bãi (Vehicle Damage Incident Report)
* **Mục tiêu:** Staff chủ động khai báo sự cố hư hại tài sản (xe xước, va quẹt) tại bãi xe.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `staff`.
  2. Vào mục **Quản lý sự cố** -> Bấm **Báo cáo sự cố mới**.
  3. Điền thông tin:
     * Chọn loại sự cố: `Mất mát/Hư hỏng tài sản` (Incident Type).
     * Liên kết phiên gửi xe: Chọn phiên hoạt động của xe `51G-12345`.
     * Nhập mô tả chi tiết: `"Xe bị xước ở đầu xe do va quẹt nhẹ tại Zone ZC01"`.
     * Tải lên hình ảnh minh chứng.
  4. Bấm **Gửi báo cáo**.
* **Kết quả mong đợi:** Sự cố được ghi nhận vào hệ thống ở trạng thái `PENDING` (Chờ xử lý), hiển thị chi tiết đầy đủ ảnh chụp và thông tin người khai báo.

---

## 🛡️ 4. Role: Admin / Manager (Tài khoản `admin` hoặc `manager`)

### Kịch bản 3.1: Hoàn tiền cọc khi hủy đặt chỗ (Refund Flow)
* **Mục tiêu:** Khách hủy đơn sớm và Admin duyệt trả lại tiền cọc cho khách.
* **Các bước thực hiện:**
  1. **Khách hàng (`driver`):** Tìm đơn đặt chỗ `CONFIRMED` -> Bấm **Hủy Đặt Chỗ** (đảm bảo thực hiện trước giờ hẹn ít nhất 60 phút).
  2. **Admin (`admin`):** Đăng nhập vào hệ thống -> Vào mục **Quản lý giao dịch** -> Tìm giao dịch có trạng thái `REFUND_PENDING` tương ứng -> Bấm **Xác nhận hoàn tiền**.
* **Kết quả mong đợi:** Trạng thái giao dịch chuyển sang **`REFUNDED`** (Đã hoàn tiền).

### Kịch bản 3.2: Thay đổi cấu hình giá & thời hạn vé tháng mới (Pricing configuration update)
* **Mục tiêu:** Đổi biểu giá và thời hạn vé tháng từ 30 ngày sang 90 ngày cho xe ô tô.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `admin`.
  2. Vào màn hình **Cấu hình bảng giá vé tháng**.
  3. Bấm **Tạo cấu hình mới**:
     * Chọn loại xe: **Car** (Ô tô)
     * Nhập giá: `2.500.000đ`
     * Nhập số ngày hiệu lực: `90` (thay vì 30 ngày cũ).
     * Bấm **Lưu cấu hình**.
* **Kết quả mong đợi:** Cấu hình giá cũ của xe ô tô tự động chuyển sang inactive. Khi tài khoản `driver` đăng ký vé tháng mới tiếp theo cho xe ô tô, hệ thống sẽ tự động hiển thị giá `2.500.000đ` và thời hạn hiệu lực là `90 ngày`.

### Kịch bản 3.3: Chặn xe vi phạm thông qua Danh sách đen (Blacklist Enforcement)
* **Mục tiêu:** Khóa xe vi phạm quy định gửi xe và kiểm tra tính hiệu quả của cơ chế chặn.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `admin`.
  2. Truy cập màn hình **Quản lý Blacklist** -> Bấm **Chặn phương tiện**.
  3. Nhập thông tin xe cần chặn: Biển số `51G-12345` (Xe của `driver`), nhập lý do: `"Không thanh toán phí đỗ xe quá hạn nhiều lần"`. Bấm **Xác nhận chặn**.
  4. Đăng nhập tài khoản `driver` và thử tạo một Booking mới cho xe `51G-12345`.
* **Kết quả mong đợi:** 
  * Tại bước 3: Xe `51G-12345` được thêm vào danh sách đen thành công.
  * Tại bước 4: Giao diện tạo đặt chỗ của khách hàng báo lỗi cảnh báo màu đỏ: `"Phương tiện đang nằm trong danh sách đen, không thể thực hiện đặt chỗ."` (Mã lỗi API: `VEHICLE_BLACKLISTED`).

### Kịch bản 3.4: Theo dõi Dashboard thống kê thời gian thực (Real-time Dashboard Tracking)
* **Mục tiêu:** Theo dõi và kiểm tra tính cập nhật tức thời của màn hình Dashboard quản trị.
* **Các bước thực hiện:**
  1. Đăng nhập tài khoản `admin` hoặc `manager`.
  2. Truy cập vào trang **Dashboard Tổng quan** và ghi nhận các số liệu hiện tại (Ví dụ: Số xe đang đỗ trong bãi: 1, tỉ lệ lấp đầy: 10%).
  3. **Mô phỏng xe mới vào:** Thực hiện Check-In thành công cho một xe lượt `51A-99999` (như Kịch bản 2.1).
  4. Quay lại trang Dashboard quản trị.
* **Kết quả mong đợi:** Số liệu tổng quan tự động tăng lên tương ứng (Số xe đang đỗ tăng từ 1 lên 2, tỷ lệ lấp đầy khu vực đỗ thay đổi tương ứng).
