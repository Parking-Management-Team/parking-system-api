# API Spec: Các Endpoint Chưa Có (Cần Triển Khai)

> Cập nhật: 2026-06-24

---

## 1. Dashboard

Tổng quan hệ thống cho màn hình Dashboard (Staff/Admin).

### GET `/api/dashboard/overview`

Lấy số liệu tổng quan: slot trống, xe đang đỗ, doanh thu hôm nay.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `buildingId` | int? | Lọc theo tòa nhà (bỏ trống = tất cả) |

**Response 200:**

```json
{
  "data": {
    "totalSlots": 200,
    "availableSlots": 85,
    "occupiedSlots": 110,
    "blockedSlots": 5,
    "activeSessions": 110,
    "todayRevenue": 15750000,
    "todayCheckins": 145,
    "todayCheckouts": 132
  }
}
```

### GET `/api/dashboard/occupancy`

Lấy thông tin occupancy theo từng tầng/zones.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `buildingId` | int? | Lọc theo tòa nhà |

**Response 200:**

```json
{
  "data": [
    {
      "floorId": 1,
      "floorName": "Tầng 1",
      "totalSlots": 50,
      "availableSlots": 20,
      "occupiedSlots": 28,
      "blockedSlots": 2,
      "zones": [
        {
          "zoneId": 1,
          "zoneName": "Zone A - Ô tô",
          "totalSlots": 30,
          "availableSlots": 10,
          "occupiedSlots": 18,
          "blockedSlots": 2
        }
      ]
    }
  ]
}
```

### GET `/api/dashboard/active-sessions`

Danh sách xe đang trong bãi (để hiển thị trên bảng quản lý).

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `buildingId` | int? | Lọc theo tòa nhà |
| `pageIndex` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 20 |

**Response 200:**

```json
{
  "data": {
    "items": [
      {
        "id": 101,
        "licensePlate": "51A-12345",
        "vehicleTypeName": "Ô tô",
        "slotCode": "A-05",
        "checkInTime": "2026-06-24T08:30:00",
        "durationMinutes": 120,
        "isMonthlySubscriber": false,
        "buildingName": "Tầng hầm B1"
      }
    ],
    "totalCount": 110,
    "pageIndex": 1,
    "pageSize": 20
  }
}
```

---

## 2. Parking Sessions — Endpoint Mới

### GET `/api/parking-sessions/by-vehicle/{vehicleId}`

Lịch sử đỗ xe theo phương tiện.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `pageIndex` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 10 |

**Response 200:**

```json
{
  "data": {
    "items": [
      {
        "id": 101,
        "licensePlate": "51A-12345",
        "vehicleTypeName": "Ô tô",
        "slotCode": "A-05",
        "checkInTime": "2026-06-24T08:30:00",
        "checkOutTime": "2026-06-24T10:30:00",
        "status": "COMPLETED",
        "totalFee": 25000
      }
    ],
    "totalCount": 15,
    "pageIndex": 1,
    "pageSize": 10
  }
}
```

### GET `/api/parking-sessions/by-account/{accountId}`

Lịch sử đỗ xe theo tài khoản.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `status` | string? | Lọc theo trạng thái: ACTIVE, COMPLETED |
| `pageIndex` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 10 |

**Response:** Giống `by-vehicle`

### GET `/api/parking-sessions/history`

Lịch sử với filter nâng cao.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `fromDate` | DateTime? | Từ ngày |
| `toDate` | DateTime? | Đến ngày |
| `buildingId` | int? | Lọc theo tòa nhà |
| `status` | string? | ACTIVE, COMPLETED |
| `search` | string? | Tìm theo biển số |
| `pageIndex` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 10 |

---

## 3. Bookings — Flow Xác Nhận

### POST `/api/bookings/{id}/confirm`

Xác nhận booking (PENDING → CONFIRMED). Dùng khi Staff xác nhận sẽ giữ chỗ cho khách.

**Response 200:**

```json
{
  "data": {
    "id": 1,
    "status": "CONFIRMED",
    "licensePlate": "51A-12345",
    "buildingName": "Tầng hầm B1",
    "plannedCheckinTime": "2026-06-25T09:00:00"
  },
  "message": "Booking confirmed successfully."
}
```

**Response 409:** Nếu booking không ở trạng thái PENDING.

---

## 4. Monthly Subscriptions — Endpoint Mới

### GET `/api/monthly-subscriptions/by-account/{accountId}`

Danh sách vé tháng theo tài khoản.

**Response 200:**

```json
{
  "data": [
    {
      "id": 1,
      "accountFullName": "Nguyễn Văn A",
      "licensePlate": "51A-12345",
      "vehicleTypeName": "Ô tô",
      "buildingName": "Tầng hầm B1",
      "assignedSlotCode": "A-05",
      "assignedCardCode": "CARD-001",
      "status": "ACTIVE",
      "activatedAt": "2026-06-01T00:00:00",
      "expiredAt": "2026-07-01T00:00:00"
    }
  ]
}
```

### GET `/api/monthly-subscriptions/by-building/{buildingId}`

Danh sách vé tháng theo tòa nhà.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `status` | string? | ACTIVE, PENDING, CANCELLED |

---

## 5. Payments — Endpoint Mới

### GET `/api/payments/by-session/{sessionId}`

Lịch sử thanh toán của một lượt đỗ xe.

**Response 200:**

```json
{
  "data": [
    {
      "id": 1,
      "sessionId": 101,
      "amount": 25000,
      "method": "CASH",
      "status": "COMPLETED",
      "createdAt": "2026-06-24T10:30:00"
    }
  ]
}
```

### GET `/api/payments/by-account/{accountId}`

Lịch sử thanh toán theo tài khoản.

### GET `/api/payments`

Danh sách tất cả giao dịch (Admin/Staff).

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `fromDate` | DateTime? | Từ ngày |
| `toDate` | DateTime? | Đến ngày |
| `method` | string? | CASH, ONLINE_BANKING |
| `pageIndex` | int | Mặc định 1 |
| `pageSize` | int | Mặc định 10 |

---

## 6. Revenue — Endpoint Mới

### GET `/api/revenue/by-building/{buildingId}`

Doanh thu theo tòa nhà.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `fromDate` | DateTime? | Từ ngày |
| `toDate` | DateTime? | Đến ngày |
| `cycleType` | string? | Daily, Weekly, Monthly |

**Response 200:**

```json
{
  "data": [
    {
      "cycleDate": "2026-06-24",
      "totalRevenue": 5250000,
      "transactionCount": 42,
      "buildingId": 1,
      "buildingName": "Tầng hầm B1"
    }
  ]
}
```

### GET `/api/revenue/summary`

Tổng hợp doanh thu theo khoảng ngày.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `fromDate` | DateTime | Bắt buộc |
| `toDate` | DateTime | Bắt buộc |
| `buildingId` | int? | Lọc theo tòa nhà |

**Response 200:**

```json
{
  "data": {
    "totalRevenue": 47500000,
    "totalTransactions": 350,
    "averagePerDay": 3392857,
    "topBuilding": "Tầng hầm B1"
  }
}
```

---

## 7. Cards — Endpoint Mới

### GET `/api/cards/available`

Danh sách thẻ đang Available (có thể gán cho vé tháng mới).

**Response 200:**

```json
{
  "data": [
    {
      "id": 5,
      "cardCode": "CARD-005",
      "rfidCode": "RF-005",
      "cardType": "MONTHLY",
      "status": "Available"
    }
  ]
}
```

### GET `/api/cards/assigned`

Danh sách thẻ đang Assigned (đang dùng cho vé tháng).

---

## 8. Reports

### GET `/api/reports/daily`

Báo cáo doanh thu theo ngày.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `date` | DateTime | Ngày cần báo cáo (mặc định hôm nay) |
| `buildingId` | int? | Lọc theo tòa nhà |

**Response 200:**

```json
{
  "data": {
    "reportDate": "2026-06-24",
    "buildingName": "Tất cả",
    "totalCheckins": 145,
    "totalCheckouts": 132,
    "currentOccupancy": 85,
    "cashRevenue": 8750000,
    "onlineRevenue": 7000000,
    "totalRevenue": 15750000,
    "monthlyCheckins": 35,
    "casualCheckins": 110
  }
}
```

### GET `/api/reports/monthly`

Báo cáo doanh thu theo tháng.

**Query params:**

| Param | Type | Mô tả |
|-------|------|-------|
| `year` | int | Năm |
| `month` | int | Tháng |
| `buildingId` | int? | Lọc theo tòa nhà |

---

## Tóm Tắt Ưu Tiên Triển Khai

| Ưu Tiên | Module | Lý Do |
|----------|--------|-------|
| **P0** | Dashboard Overview | Màn hình chính Staff/Admin cần dữ liệu real-time |
| **P0** | Parking Sessions (by-vehicle, history) | Staff tra cứu lịch sử xe ra vào |
| **P1** | Booking Confirm Flow | Cần để Staff xác nhận đặt chỗ |
| **P1** | Monthly Subscriptions (by-account, by-building) | Quản lý vé tháng |
| **P1** | Payments (by-session, by-account) | Lịch sử thanh toán |
| **P2** | Revenue (by-building, summary) | Báo cáo chi tiết |
| **P2** | Cards (available, assigned) | Quản lý thẻ |
| **P2** | Reports (daily, monthly) | Báo cáo định kỳ |
