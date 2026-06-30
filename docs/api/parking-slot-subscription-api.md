# Parking Slot API - Subscription Info

Base URL: `http://localhost:5029`

---

## 1. GET `/api/ParkingSlots/zone/{zoneId}` - List Slots by Zone

**Query Params:** `statuses`, `vehicleTypeIds`, `search`

**Response:**
```json
{
  "data": [
    {
      "id": 3,
      "zoneId": 2,
      "vehicleTypeId": 2,
      "code": "ZC01-01",
      "name": "Slot ZC01-01",
      "status": "Occupied",
      "occupiedLicensePlate": "51G-12345",
      "subscription": {
        "subscriptionId": 1,
        "accountId": 4,
        "accountName": "Bob Johnson (Driver)",
        "vehicleId": 1,
        "licensePlate": "51G-12345",
        "status": "ACTIVE",
        "monthlyPrice": 1000000,
        "activatedAt": "2026-06-04T00:00:00Z",
        "expiredAt": "2026-07-04T00:00:00Z"
      }
    },
    {
      "id": 4,
      "zoneId": 2,
      "vehicleTypeId": 2,
      "code": "ZC01-02",
      "name": "Slot ZC01-02",
      "status": "Available",
      "occupiedLicensePlate": null,
      "subscription": null
    }
  ],
  "success": true,
  "message": null
}
```

---

## 2. GET `/api/ParkingSlots/{id}` - Slot Detail

**Response:**
```json
{
  "data": {
    "id": 3,
    "zoneId": 2,
    "vehicleTypeId": 2,
    "code": "ZC01-01",
    "name": "Slot ZC01-01",
    "status": "Occupied",
    "occupiedLicensePlate": "51G-12345",
    "subscription": {
      "subscriptionId": 1,
      "accountId": 4,
      "accountName": "Bob Johnson (Driver)",
      "vehicleId": 1,
      "licensePlate": "51G-12345",
      "status": "ACTIVE",
      "monthlyPrice": 1000000,
      "activatedAt": "2026-06-04T00:00:00Z",
      "expiredAt": "2026-07-04T00:00:00Z"
    }
  },
  "success": true,
  "message": null
}
```

---

## 3. GET `/api/ParkingSlots` - List All Slots

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "zoneId": 1,
      "vehicleTypeId": 1,
      "code": "ZM01-01",
      "name": "Slot ZM01-01",
      "status": "Available",
      "occupiedLicensePlate": null,
      "subscription": null
    }
  ],
  "success": true,
  "message": null
}
```

---

## 4. GET `/api/ParkingSlots/paged` - List Slots with Pagination

**Query Params:** `pageIndex`, `pageSize`

**Response:**
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "zoneId": 1,
        "vehicleTypeId": 1,
        "code": "ZM01-01",
        "name": "Slot ZM01-01",
        "status": "Available",
        "occupiedLicensePlate": null,
        "subscription": null
      }
    ],
    "totalCount": 10,
    "totalPages": 1,
    "pageIndex": 1,
    "pageSize": 10
  },
  "success": true,
  "message": null
}
```

---

## Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| id | int | Slot ID |
| zoneId | int | Zone ID |
| vehicleTypeId | int | Vehicle Type ID (1=Motorcycle, 2=Car) |
| code | string | Slot code (e.g., "ZC01-01") |
| name | string | Slot display name |
| status | string | Available/Occupied/Blocked/Maintenance |
| occupiedLicensePlate | string? | License plate of vehicle occupying slot |
| subscription | object? | Monthly subscription info (null if not occupied by subscriber) |

---

## Subscription Object (inside slot)

| Field | Type | Description |
|-------|------|-------------|
| subscriptionId | int | Monthly Subscription ID |
| accountId | int | Account ID (driver) |
| accountName | string | Driver's full name |
| vehicleId | int | Vehicle ID |
| licensePlate | string | Vehicle license plate |
| status | string | ACTIVE/PENDING/EXPIRED/CANCELLED |
| monthlyPrice | decimal | Monthly subscription price |
| activatedAt | DateTime? | Activation date |
| expiredAt | DateTime? | Expiration date |

---

## Subscription Status Values

| Status | Description |
|--------|-------------|
| PENDING | Waiting for payment |
| ACTIVE | Currently active subscription |
| EXPIRED | Subscription expired |
| DOWNGRADED | Downgraded |
| CANCELLED | Subscription cancelled |

---

## Slot Status Values

| Status | Description |
|--------|-------------|
| Available | Slot is free |
| Occupied | Slot has a vehicle |
| Blocked | Slot is blocked |
| Maintenance | Slot under maintenance |

---

## Notes

1. `subscription` is only populated when slot is occupied by a **monthly subscriber**
2. For regular parking sessions (non-subscription), `subscription` will be `null`
3. `occupiedLicensePlate` is populated for both subscription and regular parking sessions
4. Use `subscription.subscriptionId` to link to subscription detail page
