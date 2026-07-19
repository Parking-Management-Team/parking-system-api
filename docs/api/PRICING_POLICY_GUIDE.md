# Hướng dẫn Quản lý và Tương tác với Chính sách giá (Pricing Policy Guide)

Tài liệu này hướng dẫn cách tương tác với hệ thống quản lý chính sách giá (**Pricing Policy**) và cấu hình tính toán phí đỗ xe trong dự án PBMS.

---

## 📌 1. Tổng quan về Chính sách giá (Pricing Policy)

Hệ thống quản lý giá của PBMS bao gồm 3 thành phần liên kết chặt chẽ:
1. **Pricing Policy (Chính sách giá):** Thực thể chính đại diện cho một bảng giá áp dụng cho một loại phương tiện (`VehicleTypeId`) trong một khoảng thời gian hiệu lực (`EffectiveStart` -> `EffectiveEnd`). Có 3 trạng thái: `Inactive` (nháp), `Active` (đang chạy), và `Expired` (đã quá hạn).
2. **Pricing Window (Khung giờ giá):** Các khung giờ trong ngày (ví dụ: Ngày 06:00 - 22:00, Đêm 22:00 - 06:00). Tổng các khung giờ trong một chính sách **bắt buộc phải phủ đủ 24 tiếng** và **không chồng lấn**.
3. **Pricing Rules (Quy tắc giá):** Các quy tắc cấu hình cụ thể cho từng chính sách như: phí cơ bản (`BasePricing`), phí lũy tiến block tiếp theo (`IncrementPricing`), phí trần tối đa theo ngày (`DailyCap`), thời gian ân hạn miễn phí (`GracePeriod`).

---

## 🔄 2. Quy trình Cài đặt & Kích hoạt Chính sách mới (Scenario 1)

Do có các quy tắc nghiệp vụ nghiêm ngặt về mặt thời gian, quy trình thêm một chính sách giá mới luôn tuân thủ **2 bước**:

### Bước 1: Tạo chính sách ở dạng nháp (`Inactive`)
Gửi request tạo chính sách kèm các khung giờ và quy tắc ban đầu. Lúc này trạng thái là `Inactive` để bạn tự do chỉnh sửa các khung giờ.
* **API:** `POST /api/pricing-policies`
* **Body mẫu:**
```json
{
  "vehicleTypeId": 2,
  "policyName": "Bảng giá Ô tô Mùa hè 2026",
  "effectiveStart": "2026-07-01T00:00:00Z",
  "effectiveEnd": null,
  "priority": 0,
  "pricingWindows": [
    {
      "windowName": "Khung giờ ngày",
      "startTime": "06:00:00",
      "endTime": "22:00:00",
      "baseDurationMinutes": 60,
      "basePrice": 20000,
      "incrementBlockMinutes": 15,
      "incrementPrice": 5000,
      "gracePeriodMinutes": 15
    },
    {
      "windowName": "Khung giờ đêm",
      "startTime": "22:00:00",
      "endTime": "06:00:00",
      "baseDurationMinutes": 60,
      "basePrice": 40000,
      "incrementBlockMinutes": 30,
      "incrementPrice": 10000,
      "gracePeriodMinutes": 15
    }
  ],
  "pricingRules": [
    {
      "ruleType": "BasePricing",
      "isActive": true,
      "executionOrder": 1,
      "basePricingRuleConfig": {
        "baseDurationMinutes": 60,
        "basePriceAmount": 20000
      }
    },
    {
      "ruleType": "IncrementPricing",
      "isActive": true,
      "executionOrder": 2,
      "incrementPricingRuleConfig": {
        "incrementIntervalMinutes": 15,
        "incrementPriceAmount": 5000,
        "thresholdPercentage": 50
      }
    }
  ]
}
```

### Bước 2: Tự do điều chỉnh Khung giờ (Nếu cần)
Khi chính sách ở trạng thái `Inactive`, Admin/Manager có thể gọi các API sau để chỉnh sửa khung giờ:
* **Thêm khung giờ:** `POST /api/pricing-policies/{id}/windows`
* **Cập nhật khung giờ:** `PUT /api/pricing-policies/windows/{windowId}`
* **Xóa khung giờ:** `DELETE /api/pricing-policies/windows/{windowId}` (Không được phép xóa nếu chỉ còn 1 khung giờ duy nhất).

### Bước 3: Kích hoạt chính sách (`Active`)
Sau khi đã kiểm tra các khung giờ phủ đủ 24h và không có sai sót, tiến hành kích hoạt để chính thức áp dụng tính tiền.
* **API:** `POST /api/pricing-policies/{id}/activate`
* **Quy tắc hệ thống kiểm tra lúc này:**
  * **BR-FEE-027 / 028:** Hệ thống kiểm tra tổng các khung giờ đỗ có bằng đúng 24h (1440 phút) hay không, có bị trống khoảng nào hoặc có bị chồng lấn nhau không.
  * **BR-FEE-025:** Khoảng thời gian hiệu lực (`EffectiveStart` -> `EffectiveEnd`) của chính sách này không được chồng lấn với bất kỳ chính sách nào đang `Active`/`Inactive` khác của cùng loại xe và **cùng độ ưu tiên (`Priority`)**.
* Sau khi kích hoạt thành công, chính sách chuyển sang **`Active`**. Đồng thời, hệ thống sẽ **khóa toàn bộ cấu hình giá** của chính sách này (không cho phép sửa khung giờ, sửa giá cơ bản/lũy tiến nữa để tránh gian lận số liệu).

---

## ⚙️ 3. Cấu hình các Chế độ Tính tiền (`APPLY_SEGMENTED_PRICING`)

Hệ thống hỗ trợ 2 chế độ tính tiền đỗ xe. Admin/Manager có thể chuyển đổi dễ dàng thông qua bảng cấu hình hệ thống bằng cách gọi API cập nhật cấu hình:

* **API Cập nhật Cấu hình:** `POST /api/configs` (hoặc thông qua Database Table `parking_system_config`).
* **Key cấu hình:** `APPLY_SEGMENTED_PRICING`
* **Giá trị cấu hình:**
  * `"false"` (Mặc định): **Chế độ tính theo giá Check-in.**
    * Hệ thống lấy duy nhất 1 chính sách giá hoạt động tại thời điểm xe bắt đầu vào bãi (Check-in) để tính tiền cho toàn bộ phiên đỗ.
    * *Fallback an toàn:* Nếu lúc xe ra bãi (Check-out) mà chính sách lúc Check-in đã hết hạn và bị chuyển thành `Expired`, hệ thống vẫn tự động tìm lại chính sách `Expired` đó để tính tiền bình thường thay vì báo lỗi.
  * `"true"`: **Chế độ tính phí phân đoạn (Segmented Pricing).**
    * Tự động chia nhỏ phiên đỗ xe của khách hàng theo các mốc chuyển giao chính sách thực tế (ví dụ: đỗ xuyên qua ngày thường và ngày Lễ/Tết).
    * Phân đoạn nào thuộc ngày thường tính theo giá ngày thường, phân đoạn nào thuộc ngày Lễ tính theo giá ngày Lễ.
    * Đảm bảo khách hàng **chỉ bị tính phí cơ bản (Base charge) duy nhất 1 lần** ở đầu phiên đỗ, các phân đoạn tiếp theo chỉ tính phí lũy tiến (Increment charge).

---

## 📅 4. Cách thiết lập Chính sách Ngày Lễ/Đặc biệt (Holiday/Event Policy)

Khi bạn muốn thiết lập bảng giá đắt hơn áp dụng riêng cho các ngày Lễ/Tết mà không muốn sửa đổi hay đóng bảng giá mặc định hiện tại, hãy sử dụng trường **`Priority`**:

1. **Bảng giá mặc định:** Thường để `Priority = 0`. Ngày hiệu lực lâu dài (ví dụ: `EffectiveStart = 01/01/2026`, `EffectiveEnd = null`).
2. **Bảng giá ngày Tết (Ví dụ 3 ngày 01/01 -> 03/01):**
   * Đặt `Priority = 1` (cao hơn 0).
   * Đặt khoảng hiệu lực: `EffectiveStart = 01/01/2026`, `EffectiveEnd = 03/01/2026`.
   * Trạng thái kích hoạt: `Active`.
3. **Kết quả hoạt động:**
   * Do `Priority` của bảng giá Tết cao hơn, hệ thống sẽ tự động chọn bảng giá Tết để tính phí cho các xe đỗ trong khoảng thời gian từ 01/01 đến 03/01.
   * Từ ngày 04/01 trở đi, hệ thống tự động quay về áp dụng bảng giá mặc định (`Priority = 0`) mà bạn không cần phải thực hiện bất kỳ thao tác thủ công nào để mở lại bảng giá mặc định.

---

## 🗂️ 5. Danh sách các API Endpoints chi tiết

| Method | Endpoint | Quyền hạn | Mô tả |
| :--- | :--- | :--- | :--- |
| **POST** | `/api/pricing-policies` | Admin, Manager | Tạo mới chính sách giá nháp (`Inactive`). |
| **POST** | `/api/pricing-policies/{id}/activate` | Admin, Manager | Kích hoạt chính sách giá từ `Inactive` -> `Active`. |
| **GET** | `/api/pricing-policies` | Mọi user | Lấy danh sách toàn bộ chính sách giá (hỗ trợ lọc theo `vehicleTypeId`, `status`). |
| **GET** | `/api/pricing-policies/{id}` | Mọi user | Lấy thông tin chi tiết một chính sách giá và các khung giờ của nó. |
| **PUT** | `/api/pricing-policies/{id}` | Admin, Manager | Cập nhật thông tin cơ bản của chính sách (Tên, Ngày hiệu lực, Trạng thái, Độ ưu tiên). |
| **POST** | `/api/pricing-policies/{id}/windows` | Admin, Manager | Thêm một khung giờ mới vào chính sách giá (chỉ khi ở trạng thái `Inactive`). |
| **PUT** | `/api/pricing-policies/windows/{windowId}` | Admin, Manager | Cập nhật khung giờ tính phí (chỉ khi ở trạng thái `Inactive`). |
| **DELETE** | `/api/pricing-policies/windows/{windowId}`| Admin, Manager | Xóa một khung giờ tính phí (chỉ khi ở trạng thái `Inactive`). |
| **POST** | `/api/pricing-policies/cleanup` | Admin, Manager | Thủ công kích hoạt dọn dẹp chuyển các chính sách quá hạn `Active` -> `Expired` (Background worker cũng tự động chạy mỗi 12 giờ). |

---

## 🎨 6. Gợi ý thiết kế Giao diện & Trải nghiệm người dùng (FE UX/UI Guidelines)

Để tối ưu hóa trải nghiệm của người quản lý bãi xe (Manager/Admin), Front-End nên thiết kế giao diện tương tác theo các nguyên tắc sau:

### 1. Thêm trường nhập Độ ưu tiên (`Priority`)
* **Kiểu dữ liệu:** Ô nhập số (Number Input) hoặc Lựa chọn độ ưu tiên (Dropdown/Slider).
* **Giá trị mặc định:** Luôn để mặc định là `0` (ngày thường).
* **Mẹo UX:** Thêm một tooltip giải thích: *"Mặc định là 0. Hãy đặt giá trị cao hơn (ví dụ: 1, 2) đối với các bảng giá ngày Lễ, Tết để tự động đè lên bảng giá ngày thường trong khoảng thời gian diễn ra sự kiện."*

### 2. Xử lý thông minh khi trùng ngày hiệu lực (`POLICY_OVERLAP`)
* Khi Manager kích hoạt chính sách mới và Backend trả về mã lỗi `POLICY_OVERLAP`, hãy kiểm tra xem lỗi này có phải do chính sách cũ đã hết hạn thực tế nhưng chưa được dọn dẹp hay không.
* **Gợi ý Popup/Modal xử lý:**
  * Hiển thị cảnh báo: *"Trùng khoảng ngày hiệu lực với chính sách cũ. Có thể chính sách cũ đã hết hạn thực tế nhưng chưa được cập nhật trạng thái."*
  * Cung cấp nút hành động: **"Dọn dẹp & Kích hoạt ngay"**.
  * **Hành vi gọi API:** FE sẽ gọi API `POST /api/pricing-policies/cleanup` trước để chuyển chính sách cũ sang `Expired`, sau đó gọi tiếp API `POST /api/pricing-policies/{id}/activate` để kích hoạt chính sách mới mà người dùng không cần thực hiện thủ công hay reload lại trang.

