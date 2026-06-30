# Vehicle Types API - Buffer Ratio Configuration

Base URL: `http://localhost:5029`

---

## 1. GET `/api/vehicle-types` - List All Vehicle Types

**Response:**
```json
{
  "data": [
    {
      "id": 2,
      "typeName": "Motorcycle",
      "vehicleTypeCode": "MOTOR",
      "description": "2-wheel motorcycle",
      "vehicleTypeStatus": "Active",
      "bufferRatio": 5
    },
    {
      "id": 3,
      "typeName": "Car",
      "vehicleTypeCode": "CAR",
      "description": "4-7 seat passenger car",
      "vehicleTypeStatus": "Active",
      "bufferRatio": 15
    }
  ],
  "success": true,
  "message": null
}
```

---

## 2. GET `/api/vehicle-types/{id}` - Vehicle Type Detail

**Response:**
```json
{
  "data": {
    "id": 3,
    "typeName": "Car",
    "vehicleTypeCode": "CAR",
    "description": "4-7 seat passenger car",
    "vehicleTypeStatus": "Active",
    "bufferRatio": 15
  },
  "success": true,
  "message": null
}
```

---

## 3. POST `/api/vehicle-types` - Create Vehicle Type

**Request Body:**
```json
{
  "typeName": "Bicycle",
  "vehicleTypeCode": "BIKE",
  "description": "Standard bicycle",
  "vehicleTypeStatus": "Active",
  "bufferRatio": 0
}
```

**Response:**
```json
{
  "data": {
    "id": 4,
    "typeName": "Bicycle",
    "vehicleTypeCode": "BIKE",
    "description": "Standard bicycle",
    "vehicleTypeStatus": "Active",
    "bufferRatio": 0
  },
  "success": true,
  "message": "Vehicle type created successfully."
}
```

---

## 4. PUT `/api/vehicle-types/{id}` - Update Vehicle Type (Admin/Manager)

**Request Body:**
```json
{
  "typeName": "Car (Modified)",
  "vehicleTypeCode": "CAR",
  "description": "4-7 seat passenger car and SUVs",
  "vehicleTypeStatus": "Active",
  "bufferRatio": 20
}
```

**Response:**
```json
{
  "data": {
    "id": 3,
    "typeName": "Car (Modified)",
    "vehicleTypeCode": "CAR",
    "description": "4-7 seat passenger car and SUVs",
    "vehicleTypeStatus": "Active",
    "bufferRatio": 20
  },
  "success": true,
  "message": "Vehicle type updated successfully."
}
```

---

## Field Descriptions

| Field | Type | Description |
|-------|------|-------------|
| id | int | Vehicle Type ID |
| typeName | string | Name of the vehicle type (e.g. Motorcycle, Car) |
| vehicleTypeCode | string | Code for vehicle type (e.g. MOTOR, CAR) |
| description | string | Detailed description of the type |
| vehicleTypeStatus | string | Status: Active/Inactive |
| bufferRatio | int | The percentage of total slots reserved for unexpected situations (e.g. 15 for 15%). Managed by Admin/Manager to control capacity availability for bookings. |

---

## Notes

1. **Effective Capacity Rule**:
   When creating a booking, the system calculates effective capacity to reserve slots for unexpected walk-ins:
   $$\text{Effective Capacity} = \text{Total Slots} - \lceil\text{Total Slots} \times (\text{BufferRatio} / 100)\rceil$$
   If active bookings overlap with the requested time frame, the booking is rejected if the count reaches or exceeds `Effective Capacity`.
