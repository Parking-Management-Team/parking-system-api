# Subscription Price Configuration API

Base URL: `http://localhost:5029`

Tài liệu này đặc tả các API quản lý cấu hình giá và thời hạn vé tháng (`SubscriptionPriceConfig`) dành cho Admin và Manager.

---

## 1. GET `/api/subscription-price-configs` - Danh sách cấu hình giá

Lấy danh sách tất cả các cấu hình giá vé tháng trong hệ thống. Hỗ trợ lọc theo loại xe và trạng thái.

**Query Params:**
* `vehicleTypeId` (int, optional): Lọc theo ID loại xe.
* `onlyActive` (bool, optional): Lọc các cấu hình đang hoạt động (`true`).

**Response (HTTP 200):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "vehicleTypeId": 1,
      "vehicleTypeName": "Xe Máy",
      "price": 150000.0,
      "durationDays": 30,
      "effectiveFrom": "2026-06-29T05:19:03Z",
      "effectiveTo": null,
      "isActive": true
    }
  ],
  "message": "Success",
  "errorCode": null,
  "errors": null
}
```

---

## 2. GET `/api/subscription-price-configs/active/{vehicleTypeId}` - Cấu hình giá đang hoạt động

Lấy cấu hình giá và thời hạn vé tháng hiện hành đang hoạt động của một loại xe cụ thể.

**Response (HTTP 200):**
```json
{
  "success": true,
  "data": {
    "id": 2,
    "vehicleTypeId": 2,
    "vehicleTypeName": "Ô Tô",
    "price": 1000000.0,
    "durationDays": 30,
    "effectiveFrom": "2026-06-29T05:19:03Z",
    "effectiveTo": null,
    "isActive": true
  },
  "message": "Success",
  "errorCode": null,
  "errors": null
}
```

---

## 3. POST `/api/subscription-price-configs` - Tạo cấu hình giá & thời hạn mới (Admin/Manager)

Tạo cấu hình giá vé mới cho loại xe. Hệ thống sẽ tự động vô hiệu hóa (set `isActive = false` và cập nhật `effectiveTo`) cấu hình cũ đang hoạt động.

**Request Body:**
```json
{
  "vehicleTypeId": 2,
  "price": 1200000.0,
  "durationDays": 90
}
```

* **`durationDays`** (int, default = 30): Số ngày hiệu lực của vé tháng khi đăng ký mới hoặc gia hạn dưới cấu hình giá này.

**Response (HTTP 201):**
```json
{
  "success": true,
  "data": {
    "id": 3,
    "vehicleTypeId": 2,
    "vehicleTypeName": "Ô Tô",
    "price": 1200000.0,
    "durationDays": 90,
    "effectiveFrom": "2026-06-29T12:22:00Z",
    "effectiveTo": null,
    "isActive": true
  },
  "message": "Subscription price configuration created successfully.",
  "errorCode": null,
  "errors": null
}
```

---

## 4. PUT `/api/subscription-price-configs/{id}/deactivate` - Vô hiệu hóa cấu hình giá (Admin/Manager)

Chuyển trạng thái hoạt động của cấu hình giá sang không hoạt động (`isActive = false`) và cập nhật thời điểm kết thúc hiệu lực.

**Response (HTTP 200):**
```json
{
  "success": true,
  "data": "3",
  "message": "Subscription price configuration deactivated successfully.",
  "errorCode": null,
  "errors": null
}
```

---

## 5. DELETE `/api/subscription-price-configs/{id}` - Xóa mềm cấu hình giá (Admin/Manager)

Đánh dấu xóa mềm cấu hình giá vé tháng khỏi hệ thống (`IsDeleted = true`).

**Response (HTTP 200):**
```json
{
  "success": true,
  "data": "3",
  "message": "Subscription price configuration soft deleted successfully.",
  "errorCode": null,
  "errors": null
}
```
