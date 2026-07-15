# ⚙️ Parking System Configuration API Reference

Allows management of system-wide configuration settings as key-value pairs. Predefined keys include `BUFFER_TIME_MINUTES` and `WALKIN_STAY_THRESHOLD_HOURS`.

---

## 1. GET `/api/ParkingSystemConfig` - List All Configurations

Retrieve a list of all active configurations in the system.

* **Method:** `GET`
* **Path:** `/api/ParkingSystemConfig`
* **Response (Success - `200 OK`):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "key": "BUFFER_TIME_MINUTES",
      "value": "30",
      "description": "Buffer time in minutes between consecutive bookings on the same slot.",
      "updatedAt": "2025-01-01T00:00:00Z",
      "updatedBy": null
    },
    {
      "key": "WALKIN_STAY_THRESHOLD_HOURS",
      "value": "2",
      "description": "Hours threshold: if booking starts within this many hours from now, walk-in car count is included in zone capacity check.",
      "updatedAt": "2025-01-01T00:00:00Z",
      "updatedBy": null
    }
  ]
}
```

---

## 2. GET `/api/ParkingSystemConfig/{key}` - Get Config Detail

Retrieve details of a single configuration setting by its key.

* **Method:** `GET`
* **Path:** `/api/ParkingSystemConfig/BUFFER_TIME_MINUTES`
* **Response (Success - `200 OK`):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "key": "BUFFER_TIME_MINUTES",
    "value": "30",
    "description": "Buffer time in minutes between consecutive bookings on the same slot.",
    "updatedAt": "2025-01-01T00:00:00Z",
    "updatedBy": null
  }
}
```
* **Response (Not Found - `404 Not Found`):**
```json
{
  "success": false,
  "errorCode": "CONFIG_NOT_FOUND",
  "message": "Configuration key 'INVALID_KEY' was not found."
}
```

---

## 3. PUT `/api/ParkingSystemConfig` - Create or Update Configuration

Create a new config setting or update an existing one (Upsert). Only accessible by `Manager` / `Admin`.

* **Method:** `PUT`
* **Path:** `/api/ParkingSystemConfig`
* **Request Body:**
```json
{
  "key": "BUFFER_TIME_MINUTES",
  "value": "15",
  "description": "Buffer time in minutes between consecutive bookings on the same slot.",
  "updatedBy": "manager-01"
}
```
* **Response (Success - `200 OK`):**
```json
{
  "success": true,
  "message": "Configuration updated successfully.",
  "data": {
    "key": "BUFFER_TIME_MINUTES",
    "value": "15",
    "description": "Buffer time in minutes between consecutive bookings on the same slot.",
    "updatedAt": "2026-07-15T01:30:00Z",
    "updatedBy": "manager-01"
  }
}
```
