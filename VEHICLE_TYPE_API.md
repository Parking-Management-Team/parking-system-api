# Vehicle Type API Documentation

## Overview
The Vehicle Type API provides endpoints for managing vehicle types (Xe Máy, Ô Tô) in the parking system. These endpoints allow managers to create, read, update, and delete vehicle types with proper validation and authorization.

## Endpoints

### 1. Get All Vehicle Types
**Endpoint:** `GET /api/vehicletype`  
**Authorization:** Required (User must be authenticated)  
**Description:** Retrieves a list of all vehicle types with their current status.

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lấy danh sách 2 loại xe thành công.",
  "data": [
    {
      "id": 1,
      "name": "Xe Máy",
      "status": "Active",
      "isActive": true,
      "createdAt": "2026-06-03T09:42:33.516Z"
    },
    {
      "id": 2,
      "name": "Ô Tô",
      "status": "Active",
      "isActive": true,
      "createdAt": "2026-06-03T09:42:33.516Z"
    }
  ]
}
```

---

### 2. Get Vehicle Type by ID
**Endpoint:** `GET /api/vehicletype/{id}`  
**Authorization:** Required  
**Path Parameters:**
- `id` (integer): Vehicle type ID

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Lấy thông tin loại xe thành công.",
  "data": {
    "id": 1,
    "name": "Xe Máy",
    "status": "Active",
    "isActive": true,
    "createdAt": "2026-06-03T09:42:33.516Z"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "errorCode": "NOT_FOUND",
  "message": "Loại xe với ID 99 không tồn tại.",
  "data": null
}
```

---

### 3. Create Vehicle Type
**Endpoint:** `POST /api/vehicletype`  
**Authorization:** Required (Manager/Admin role recommended)  
**Content-Type:** `application/json`

**Request Body:**
```json
{
  "name": "Xe Đạp"
}
```

**Response (201 Created):**
```json
{
  "success": true,
  "message": "Tạo loại xe 'Xe Đạp' thành công.",
  "data": {
    "id": 3,
    "name": "Xe Đạp",
    "status": "Active",
    "isActive": true,
    "createdAt": "2026-06-03T10:00:00.000Z"
  }
}
```

**Response (400 Bad Request - Name Already Exists):**
```json
{
  "success": false,
  "errorCode": "NAME_EXISTS",
  "message": "Loại xe 'Xe Máy' đã tồn tại trong hệ thống.",
  "data": null
}
```

**Response (400 Bad Request - Empty Name):**
```json
{
  "success": false,
  "errorCode": "INVALID_NAME",
  "message": "Tên loại xe không được để trống.",
  "data": null
}
```

---

### 4. Update Vehicle Type
**Endpoint:** `PUT /api/vehicletype/{id}`  
**Authorization:** Required (Manager/Admin role recommended)  
**Content-Type:** `application/json`  
**Path Parameters:**
- `id` (integer): Vehicle type ID

**Request Body:**
```json
{
  "name": "Xe Máy Lớn",
  "isActive": false
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Cập nhật loại xe 'Xe Máy Lớn' thành công.",
  "data": {
    "id": 1,
    "name": "Xe Máy Lớn",
    "status": "Inactive",
    "isActive": false,
    "createdAt": "2026-06-03T09:42:33.516Z"
  }
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "errorCode": "NOT_FOUND",
  "message": "Loại xe với ID 99 không tồn tại.",
  "data": null
}
```

**Response (400 Bad Request - Duplicate Name):**
```json
{
  "success": false,
  "errorCode": "NAME_EXISTS",
  "message": "Loại xe 'Ô Tô' đã tồn tại trong hệ thống.",
  "data": null
}
```

---

### 5. Delete Vehicle Type
**Endpoint:** `DELETE /api/vehicletype/{id}`  
**Authorization:** Required (Manager/Admin role recommended)  
**Path Parameters:**
- `id` (integer): Vehicle type ID

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Xóa loại xe 'Xe Đạp' thành công.",
  "data": null
}
```

**Response (404 Not Found):**
```json
{
  "success": false,
  "errorCode": "NOT_FOUND",
  "message": "Loại xe với ID 99 không tồn tại.",
  "data": null
}
```

**Response (400 Bad Request - In Use in Sessions):**
```json
{
  "success": false,
  "errorCode": "IN_USE_SESSIONS",
  "message": "Không thể xóa loại xe 'Xe Máy' vì đang được sử dụng trong các lượt gửi xe đang hoạt động.",
  "data": null
}
```

**Response (400 Bad Request - In Use in Bookings):**
```json
{
  "success": false,
  "errorCode": "IN_USE_BOOKINGS",
  "message": "Không thể xóa loại xe 'Ô Tó' vì đang được liên kết với các mã đặt chỗ chưa hoàn thành.",
  "data": null
}
```

---

## Error Codes

| Error Code | HTTP Status | Description |
|-----------|------------|-------------|
| `NOT_FOUND` | 404 | Vehicle type does not exist |
| `NAME_EXISTS` | 400 | Vehicle type name already exists |
| `INVALID_NAME` | 400 | Vehicle type name is empty or invalid |
| `IN_USE_SESSIONS` | 400 | Vehicle type is linked to active parking sessions |
| `IN_USE_BOOKINGS` | 400 | Vehicle type is linked to incomplete bookings |
| `INTERNAL_ERROR` | 500 | Internal server error |

---

## Status Values

- **Active** - Vehicle type is active and can be used (isActive = true)
- **Inactive** - Vehicle type is inactive and cannot be used (isActive = false)

---

## Default Vehicle Types

The system comes with two default vehicle types:
1. **Xe Máy** (Motorcycle) - ID: 1
2. **Ô Tó** (Car) - ID: 2

---

## Notes

- All endpoints require authentication
- Request/response bodies use camelCase for property names
- Timestamps are in UTC format (ISO 8601)
- Vehicle type names must be unique within the system
- A vehicle type cannot be deleted if it is used in active parking sessions or incomplete bookings
