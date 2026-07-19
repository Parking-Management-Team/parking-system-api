# 🧪 PBMS Pricing Engine Test Scenarios

Tài liệu này chứa các kịch bản kiểm thử (Test Scenarios) dành cho **Pricing Engine (Bộ tính phí đỗ xe áp dụng quy tắc)**. Dữ liệu dưới đây được xây dựng dựa trên thông số mặc định của Seed Data.

---

## 🛠️ Chuẩn bị trước khi test
1. Lấy JWT Token từ API đăng nhập bằng cách thực hiện:
   * Gửi request `POST /api/auth/login` với email `staff@pbms.com`.
2. Mở trình duyệt F12 -> **Console** -> Dán code sau để đính kèm Token tự động vào Swagger:
   ```javascript
   const token = "..."; // Token của bạn
   const originalFetch = window.ui.fn.fetch;

   window.ui.fn.fetch = (...args) => {
       let req = args[0];
       if (req) {
           req.headers = {
               ...req.headers,
               "Authorization": `Bearer ${token}`
           }; 
       }
       return originalFetch(...args);
   };
   ```

---

## 🚗 Các Kịch Bản Test (Endpoint: `/api/pricing-engine/calculate`)

### 📋 Cấu hình xe ô tô mặc định (Vehicle Type ID: 3 - Car)
* **Base Pricing**: 60 phút đầu giá `20,000 VND`.
* **Increment Block**: Mỗi 15 phút tiếp theo giá `5,000 VND`.
* **Grace Period**: 15 phút (ân hạn cho block phụ tiếp theo).
* **Threshold (Ngưỡng)**: 50% thời gian block lẻ.
* **Daily Cap (Giá trần)**: `150,000 VND` / ngày.

---

### 1️⃣ Kịch bản 1: Gửi xe siêu ngắn (Vào bãi đi ra ngay)
* **Mục tiêu**: Kiểm chứng quy tắc: Vào bãi là tính phí Base Price ngay lập tức, không áp dụng Grace Period cho block đầu.
* **Tham số**:
  * `vehicleTypeId`: `3`
  * `checkIn`: `2026-07-01T08:00:00`
  * `checkOut`: `2026-07-01T08:05:00` (Đỗ 5 phút)
* **Kết quả dự kiến**:
  * `totalAmount`: **`20,000`**
  * Giải thích: Phải trả đủ tiền block đầu tiên.

### 2️⃣ Kịch bản 2: Vượt block đầu nhưng nằm trong Grace Period của block phụ
* **Mục tiêu**: Kiểm tra ân hạn 15 phút cho block phụ tiếp theo.
* **Tham số**:
  * `vehicleTypeId`: `3`
  * `checkIn`: `2026-07-01T08:00:00`
  * `checkOut`: `2026-07-01T09:10:00` (Đỗ 70 phút = 60 phút base + 10 phút lẻ)
* **Kết quả dự kiến**:
  * `totalAmount`: **`20,000`**
  * Giải thích: 10 phút lẻ $\le$ 15 phút Grace Period nên không tính tiền block lẻ đầu tiên.

### 3️⃣ Kịch bản 3: Vượt block đầu + vượt Grace Period nhưng dưới Threshold
* **Mục tiêu**: Kiểm tra tính phí khi vượt Grace Period nhưng thời gian tính phí thực tế nằm dưới ngưỡng 50% Threshold.
* **Tham số**:
  * `vehicleTypeId`: `3`
  * `checkIn`: `2026-07-01T08:00:00`
  * `checkOut`: `2026-07-01T09:20:00` (Đỗ 80 phút. Dư 20 phút sau block đầu. Trừ 15 phút Grace Period còn 5 phút tính phí. 5/15 phút = 33% $\le$ 50% Threshold)
* **Kết quả dự kiến**:
  * `totalAmount`: **`20,000`**
  * Giải thích: Thời gian lẻ thực tế dưới ngưỡng 50% block 15 phút nên được miễn phí block phụ này.

### 4️⃣ Kịch bản 4: Vượt block đầu + vượt Grace Period + vượt Threshold
* **Mục tiêu**: Đỗ vượt hẳn ngưỡng tính phí của block phụ.
* **Tham số**:
  * `vehicleTypeId`: `3`
  * `checkIn`: `2026-07-01T08:00:00`
  * `checkOut`: `2026-07-01T09:25:00` (Đỗ 85 phút. Dư 25 phút. Trừ 15 phút Grace Period còn 10 phút tính phí. 10/15 phút = 66% $\ge$ 50% Threshold)
* **Kết quả dự kiến**:
  * `totalAmount`: **`25,000`**
  * Giải thích: Tính thêm 1 block phụ `5,000 VND`.

### 5️⃣ Kịch bản 5: Đỗ xe dài ngày kích hoạt giá trần (Daily Cap)
* **Mục tiêu**: Đỗ quá 24h để kích hoạt giá trần ngày.
* **Tham số**:
  * `vehicleTypeId`: `3`
  * `checkIn`: `2026-07-01T08:00:00`
  * `checkOut`: `2026-07-02T08:00:00` (Đỗ đúng 24 tiếng)
* **Kết quả dự kiến**:
  * `totalAmount`: **`150,000`**
  * Giải thích: Tổng số tiền cộng lũy tiến vượt quá 150k nên áp dụng Daily Cap.
