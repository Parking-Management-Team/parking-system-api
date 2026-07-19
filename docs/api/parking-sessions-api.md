# Parking Sessions API - Check-in, Checkout & Penalty Surcharge

Base URL: `http://localhost:5029`

---

## 1. POST `/api/parking-sessions/check-in` - Check-in Vehicle

Checks in a vehicle based on License Plate, Card Code, and Vehicle Type. Automatically links any confirmed active Booking or Monthly Subscription.

**Request Body:**
```json
{
  "licensePlate": "51G-12345",
  "cardCode": "CARD001",
  "vehicleTypeId": 3,
  "buildingId": 1,
  "staffId": 2
}
```

**Response:**
```json
{
  "data": {
    "id": 1,
    "vehicleId": 2,
    "buildingId": 1,
    "cardId": 10,
    "zoneId": 2,
    "slotId": 15,
    "bookingId": 5,
    "bookingCode": "BK-000005",
    "monthlySubscriptionId": null,
    "inStaffId": 2,
    "outStaffId": null,
    "checkInTime": "2026-06-24T10:00:00Z",
    "checkOutTime": null,
    "licensePlateIn": "51G-12345",
    "licensePlateOut": null,
    "sessionStatus": "ACTIVE",
    "cardCode": "CARD001",
    "zoneCode": "ZC01",
    "slotCode": "ZC01-01"
  },
  "success": true,
  "message": "Vehicle checked in successfully."
}
```

---

## 2. PATCH `/api/parking-sessions/{id}/checkout/start` - Start Checkout & Evaluate Overtime

Initiates the checkout process. Calculates the total parking fee (Base + Increment Blocks + Daily Cap) and merges all active **Open** incident penalty fees via the Pricing Engine.

If the parking session was created from a Booking and the current time exceeds the `PlannedCheckoutTime` by more than **15 minutes** (grace period), the system automatically creates an **Open** incident of type `LATE_CHECKOUT` and applies the configured penalty fee to the session.

**Request Body:**
```json
{
  "checkOutTime": "2026-06-24T12:30:00Z",
  "licensePlateOut": "51G-12345",
  "outStaffId": 3
}
```

**Response:**
```json
{
  "data": {
    "id": 1,
    "vehicleId": 2,
    "buildingId": 1,
    "cardId": 10,
    "zoneId": 2,
    "slotId": 15,
    "bookingId": 5,
    "bookingCode": "BK-000005",
    "monthlySubscriptionId": null,
    "inStaffId": 2,
    "outStaffId": 3,
    "checkInTime": "2026-06-24T10:00:00Z",
    "checkOutTime": "2026-06-24T12:30:00Z",
    "licensePlateIn": "51G-12345",
    "licensePlateOut": "51G-12345",
    "sessionStatus": "ACTIVE",
    "cardCode": "CARD001",
    "zoneCode": "ZC01",
    "slotCode": "ZC01-01",
    "totalFee": 20000.0,
    "penaltyFee": 50000.0,
    "amountDue": 70000.0
  },
  "success": true,
  "message": "Started checkout successfully. Waiting for completion."
}
```

---

## 3. PATCH `/api/parking-sessions/{id}/complete` - Complete Checkout & Resolve Incidents

Completes the parking session, freeing the slot and making the card available for another check-in. This is typically invoked after the payment has been successfully recorded. All **Open** incidents of type `LOST_CARD` and `LATE_CHECKOUT` linked to this session are automatically marked as **Resolved**.

**Response:**
```json
{
  "data": {
    "id": 1,
    "sessionStatus": "COMPLETED"
  },
  "success": true,
  "message": "Completed parking session successfully."
}
```

---

## 4. PATCH `/api/parking-sessions/{id}/checkout/rollback` - Rollback Checkout

Cancels the checkout initialization, restoring the session state to active. Any **Open** `LATE_CHECKOUT` incident created during the initialization is automatically deleted from the system.

**Response:**
```json
{
  "data": {
    "id": 1,
    "checkOutTime": null,
    "licensePlateOut": null,
    "outStaffId": null,
    "sessionStatus": "ACTIVE"
  },
  "success": true,
  "message": "Rolled back checkout successfully."
}
```

---

## Overtime Surcharge Logic (BR-LATE-CHECKOUT)

1. **Grace Period**: 15 minutes.
2. **Evaluation**:
   If $CheckOutTime > Booking.PlannedCheckoutTime + 15\text{ minutes}$, look up the active penalty config for the incident code `LATE_CHECKOUT`.
3. **Incident Creation**:
   An incident of type `LATE_CHECKOUT` is created with status `Open` and is linked to the parking session.
4. **Payment Billing**:
   When payment is created for the session via `POST /api/payments`, the system automatically sums up the penalty fees of all `Open` incidents (including `LATE_CHECKOUT`) and adds them to the total invoice amount.
5. **Resolution**:
   Once the payment transitions to `PAID`, `CompleteAsync` marks all `LATE_CHECKOUT` incidents as `Resolved`.
