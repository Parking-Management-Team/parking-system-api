# 📖 Tài liệu Hướng dẫn sử dụng API - Luồng Nghiệp vụ Staff (PBMS)

Tài liệu này hướng dẫn chi tiết cách gọi và sử dụng các API mới được phát triển nhằm phục vụ luồng nghiệp vụ của nhân viên trực cổng (Staff) và quản lý (Manager).

---

## 1. Báo cáo ca trực (Shift Report)
Nghiệp vụ ca trực được tính toán động (dynamic tracking) thay vì chia ca cứng. Nhân viên cổng có thể xem trước số liệu két và nộp báo cáo ca trực khi bàn giao.

### 1.1. Xem trước số liệu ca hiện tại (Staff)
Tạm tính số lượt check-in, check-out và doanh thu dự kiến (Expected Cash) kể từ thời điểm kết thúc ca trực gần nhất cho tới hiện tại.
* **Endpoint:** `GET /api/shift-reports/preview`
* **Query Params:**
  * `staffId` (int, Required): ID tài khoản nhân viên đang trực.
* **Response:**
  ```json
  {
    "success": true,
    "message": "Shift report preview generated successfully.",
    "data": {
      "staffId": 3,
      "staffName": "Nguyen Van A",
      "startTime": "2026-07-10T08:00:00Z",
      "endTime": "2026-07-10T16:00:00Z",
      "totalCheckIn": 42,
      "totalCheckOut": 38,
      "systemRevenue": 450000.00,
      "expectedCashAmount": 300000.00
    }
  }
  ```
  *(Lưu ý: `expectedCashAmount` chỉ cộng dồn các giao dịch bằng phương thức `CASH` mà Staff này thu trực tiếp ở cổng).*

---

### 1.2. Nộp báo cáo ca trực chính thức (Staff)
Lưu trữ thông tin ca trực và đẩy thông báo cho Manager phê duyệt đối soát.
* **Endpoint:** `POST /api/shift-reports`
* **Request Body:**
  ```json
  {
    "staffId": 3,
    "actualCashAmount": 300000.00,
    "note": "Két tiền khớp hoàn toàn với hệ thống."
  }
  ```
* **Response:**
  ```json
  {
    "success": true,
    "message": "Shift report submitted successfully.",
    "data": {
      "id": 1,
      "staffId": 3,
      "staffName": "Nguyen Van A",
      "startTime": "2026-07-10T08:00:00Z",
      "endTime": "2026-07-10T16:00:00Z",
      "totalCheckIn": 42,
      "totalCheckOut": 38,
      "systemRevenue": 450000.00,
      "expectedCashAmount": 300000.00,
      "actualCashAmount": 300000.00,
      "differenceAmount": 0.00,
      "note": "Két tiền khớp hoàn toàn với hệ thống.",
      "status": "Submitted",
      "approvedById": null,
      "approvedByName": null,
      "approvedAt": null,
      "createdAt": "2026-07-10T16:01:00Z"
    }
  }
  ```

---

### 1.3. Phê duyệt/Từ chối báo cáo ca trực (Manager)
Quản lý phê duyệt đối soát tiền mặt thực tế nhận bàn giao từ nhân viên cổng.
* **Endpoint:** `POST /api/shift-reports/{id}/approve`
* **Query Params:**
  * `managerId` (int, Required): ID tài khoản Manager thực hiện duyệt.
  * `approve` (bool, Required): `true` để phê duyệt (Approved), `false` để bác bỏ (Rejected).
* **Response:**
  ```json
  {
    "success": true,
    "message": "Shift report has been Approved successfully.",
    "data": {
      "id": 1,
      "status": "Approved",
      "approvedById": 2,
      "approvedByName": "Manager B",
      "approvedAt": "2026-07-10T16:05:00Z"
    }
  }
  ```

---

### 1.4. Lấy danh sách báo cáo ca trực phân trang (Manager)
Xem lịch sử bàn giao ca trực phục vụ đối soát định kỳ.
* **Endpoint:** `GET /api/shift-reports`
* **Query Params:**
  * `pageIndex` (int, Default: 1)
  * `pageSize` (int, Default: 10)

---

## 2. Check-out không thanh toán (Unpaid Checkout)
Giải phóng cổng khẩn cấp cho xe quỵt tiền, hệ thống sẽ tự động chuyển trạng thái slot gửi xe/thẻ gửi xe, đưa cả xe và thẻ gửi xe vào Blacklist chặn đồng thời sinh incident quỵt tiền và gửi Notification cho Manager.
* **Endpoint:** `POST /api/parking-sessions/{id}/unpaid-checkout`
* **Request Body:**
  ```json
  {
    "reason": "Khách cố tình lái xe vượt rào không trả tiền."
  }
  ```
* **Response:**
  ```json
  {
    "success": true,
    "message": "Checked out as unpaid successfully. Vehicle has been blacklisted.",
    "data": {
      "id": 105,
      "cardId": 12,
      "vehicleId": 8,
      "licensePlateIn": "29A-12345",
      "checkInTime": "2026-07-10T09:00:00Z",
      "checkOutTime": "2026-07-10T11:00:00Z",
      "sessionStatus": "COMPLETED"
    }
  }
  ```

---

## 3. Báo mất thẻ và Hoàn tác (Lost Card & Rollback)
Quản lý sự cố khi khách làm rơi/mất thẻ vật lý gửi xe.

### 3.1. Báo mất thẻ (Lost Card)
Khóa thẻ, đổi trạng thái thẻ sang `Lost`, tự động tạo sự cố mất thẻ và tính gộp mức phí phạt đền bù thẻ vào tổng bill thanh toán check-out.
* **Endpoint:** `POST /api/parking-sessions/{id}/lost-card`
* **Request Body:**
  ```json
  {
    "reason": "Khách làm rơi ví mất thẻ tại bãi xe."
  }
  ```
* **Response:**
  ```json
  {
    "success": true,
    "message": "Lost card reported successfully. Penalty applied.",
    "data": {
      "id": 105,
      "sessionStatus": "ACTIVE"
    }
  }
  ```

---

### 3.2. Hoàn tác báo mất thẻ (Rollback Lost Card)
Khi khách tìm lại được thẻ hoặc bàn giao thẻ cũ trước khi thanh toán. Trả trạng thái thẻ về `Active`, hủy sự cố báo mất thẻ và gỡ bỏ toàn bộ Blacklist chặn (cho cả xe và thẻ).
* **Endpoint:** `POST /api/parking-sessions/{id}/lost-card/rollback`
* **Request Body:** Không có.
* **Response:**
  ```json
  {
    "success": true,
    "message": "Lost card report rolled back successfully. Card & Vehicle unblocked.",
    "data": {
      "id": 105,
      "sessionStatus": "ACTIVE"
    }
  }
  ```

---

## 4. Kiểm tra nhanh Blacklist tại cổng
Hỗ trợ cổng kiểm tra nhanh trạng thái chặn theo biển số xe khi camera AI scan thấy biển số.
* **Endpoint:** `GET /api/Blacklist/check-license-plate/{licensePlate}`
* **Response:**
  ```json
  {
    "success": true,
    "message": "Blacklist check retrieved successfully.",
    "data": {
      "isBlocked": true,
      "reason": "Quỵt tiền/Chưa thanh toán cho phiên gửi xe #105. Lý do: Khách cố tình lái xe vượt rào không trả tiền."
    }
  }
  ```
