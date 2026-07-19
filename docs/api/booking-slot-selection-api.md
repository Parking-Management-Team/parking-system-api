# Booking Slot Selection API

Base URL: `http://localhost:5029`

---

## 1. POST `/api/bookings` - Create Booking

Creates a new booking (status `Pending`). Drivers can optionally reserve a specific parking slot if the vehicle type is a Car. Slot reservation is rejected for Motorcycles/Motorbikes.

### Request Body:
```json
{
  "accountId": 4,
  "licensePlate": "51G-12345",
  "buildingId": 1,
  "plannedCheckinTime": "2026-06-24T15:30:00Z",
  "slotId": 3
}
```

### Responses:

#### A. 201 Created (Car Booking with valid SlotId)
```json
{
  "data": {
    "id": 12,
    "accountId": 4,
    "accountName": "Bob Johnson",
    "vehicleId": 1,
    "licensePlate": "51G-12345",
    "vehicleTypeId": 2,
    "vehicleTypeName": "Car",
    "buildingId": 1,
    "buildingName": "Main Building",
    "plannedCheckinTime": "2026-06-24T15:30:00Z",
    "depositAmount": 20000.00,
    "bookingStatus": "Pending",
    "paymentDeadline": "2026-06-24T12:51:47Z",
    "checkinGraceUntil": "2026-06-24T16:00:00Z",
    "confirmedAt": null,
    "cancelledAt": null,
    "cancelReason": null,
    "createdAt": "2026-06-24T12:36:47Z",
    "slotId": 3,
    "slotCode": "ZC01-03"
  },
  "success": true,
  "message": "Đặt chỗ được tạo thành công. Vui lòng thanh toán tiền cọc trong vòng 15 phút."
}
```

#### B. 400 Bad Request - INVALID_SLOT_SELECTION (Trying to select a slot for a Motorcycle)
If a user submits a booking for a Motorcycle (or any vehicle type that is not case-insensitively "Car") and supplies a `slotId`:
```json
{
  "success": false,
  "message": "Chỉ cho phép chọn vị trí đỗ (Slot) đối với xe ô tô."
}
```

#### C. 400 Bad Request - SLOT_NOT_FOUND (Slot does not exist or doesn't belong to the selected Building)
```json
{
  "success": false,
  "message": "Vị trí đỗ xe với ID 999 không tồn tại hoặc không thuộc Tòa nhà đã chọn."
}
```

#### D. 400 Bad Request - VEHICLE_TYPE_MISMATCH (Car booking with a motorcycle slot or vice versa)
```json
{
  "success": false,
  "message": "Vị trí đỗ xe đã chọn không tương thích với loại phương tiện của bạn."
}
```

#### E. 400 Bad Request - SLOT_NOT_AVAILABLE (Slot status is Blocked or Maintenance)
```json
{
  "success": false,
  "message": "Vị trí đỗ xe đã chọn hiện đang bị khóa hoặc bảo trì."
}
```

#### F. 400 Bad Request - SLOT_ALREADY_RESERVED (Slot has already been reserved by another active booking in overlapping timeframe)
```json
{
  "success": false,
  "message": "Vị trí đỗ xe đã chọn đã được đặt trước bởi khách hàng khác trong khung giờ này."
}
```

---

## 2. GET `/api/bookings/{id}` - Get Booking Detail

Retrieves detailed information of a booking, including the optional reserved slot details if specified.

### Response:
```json
{
  "data": {
    "id": 12,
    "accountId": 4,
    "accountName": "Bob Johnson",
    "vehicleId": 1,
    "licensePlate": "51G-12345",
    "vehicleTypeId": 2,
    "vehicleTypeName": "Car",
    "buildingId": 1,
    "buildingName": "Main Building",
    "plannedCheckinTime": "2026-06-24T15:30:00Z",
    "depositAmount": 20000.00,
    "bookingStatus": "Pending",
    "paymentDeadline": "2026-06-24T12:51:47Z",
    "checkinGraceUntil": "2026-06-24T16:00:00Z",
    "confirmedAt": null,
    "cancelledAt": null,
    "cancelReason": null,
    "createdAt": "2026-06-24T12:36:47Z",
    "slotId": 3,
    "slotCode": "ZC01-03"
  },
  "success": true,
  "message": null
}
```

---

## 3. GET `/api/bookings` - List Bookings

Retrieves all bookings with pagination or filtering where configured. Returns list of bookings with slot details.

### Response:
```json
{
  "data": [
    {
      "id": 12,
      "accountId": 4,
      "accountName": "Bob Johnson",
      "vehicleId": 1,
      "licensePlate": "51G-12345",
      "vehicleTypeId": 2,
      "vehicleTypeName": "Car",
      "buildingId": 1,
      "buildingName": "Main Building",
      "plannedCheckinTime": "2026-06-24T15:30:00Z",
      "depositAmount": 20000.00,
      "bookingStatus": "Pending",
      "paymentDeadline": "2026-06-24T12:51:47Z",
      "checkinGraceUntil": "2026-06-24T16:00:00Z",
      "confirmedAt": null,
      "cancelledAt": null,
      "cancelReason": null,
      "createdAt": "2026-06-24T12:36:47Z",
      "slotId": 3,
      "slotCode": "ZC01-03"
    }
  ],
  "success": true,
  "message": null
}
```

---

## Field Descriptions (Booking Model)

| Field | Type | Description |
|-------|------|-------------|
| id | int | Booking ID |
| accountId | int | ID of the driver account |
| accountName | string? | Driver account's full name |
| vehicleId | int | ID of the booking vehicle |
| licensePlate | string | License plate of the booking vehicle |
| vehicleTypeId | int | Vehicle type ID |
| vehicleTypeName | string? | Name of vehicle type (e.g., "Car", "Motorcycle") |
| buildingId | int | Building ID |
| buildingName | string? | Name of the building |
| plannedCheckinTime | DateTime | Expected entry time |
| depositAmount | decimal | Base deposit amount matching PricingWindow pricing |
| bookingStatus | string | Status: Pending / Confirmed / CheckedIn / Cancelled / NoShow / Expired |
| paymentDeadline | DateTime | Expiry time for deposit payment (15 minutes from creation) |
| checkinGraceUntil | DateTime | Cutoff checkin time (30 minutes after planned check-in time) |
| confirmedAt | DateTime? | Confirmation timestamp after deposit is paid |
| cancelledAt | DateTime? | Cancellation timestamp |
| cancelReason | string? | Reason for cancellation |
| slotId | int? | ID of the reserved slot. Only allowed and optional for Cars (null for Motorcycles/Motorbikes or when unspecified) |
| slotCode | string? | Code of the reserved slot (e.g., "ZC01-03") |

---

## Business Logic & Constraints

1. **Motorcycle Bookings**: Cannot select specific slots because there are no specific physical slot divisions for motorcycles in the layout. Any motorcycle booking request with a non-null `slotId` will fail with code `INVALID_SLOT_SELECTION`.
2. **Car Bookings**: Can optionally specify a target slot in `slotId`. The slot will be reserved exclusively for that booking during the expected check-in interval if it passes all domain validation checks.
3. **Automatic Assignment at Check-in**:
   * During vehicle check-in (`POST /api/parking-sessions/check-in`), if a matching confirmed booking has a reserved slot (`SlotId`), the system will automatically allocate that specific slot to the parking session and mark it as `Occupied` in the database.
   * If the car booking has no reserved slot (or check-in is done without pre-reserved slot booking), the system automatically searches for and assigns a general slot.
   * Motorcycles are checked into a zone directly without slot assignment (the session `SlotId` will remain `null`).
