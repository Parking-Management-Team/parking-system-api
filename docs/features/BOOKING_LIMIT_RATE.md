# 🚗 Booking Limit Rate per Zone & Auto Delay Fallback

## 1. Feature Description

To prevent overbooking and handle vehicle checkout delays gracefully, the parking building management system introduces:
1. **`BookingLimitRate` per Zone**: A manager-configured percentage of zone capacity that restricts how many slots can be pre-booked concurrently, preserving a buffer for walk-ins and delay handling.
2. **Auto Delay Fallback**: If a driver with a confirmed booking arrives to check in but their designated slot is still occupied by a delayed vehicle, the system dynamically reassigns them to an available fallback slot in the same Zone.
3. **`ParkingSystemConfig` Table**: A dynamic system configuration table to store properties like `BUFFER_TIME_MINUTES` and `WALKIN_STAY_THRESHOLD_HOURS` which can be updated at runtime without recompiling.

---

## 2. Logic Flows

### 2.1 Zone-Level Capacity check at Booking Creation
When a customer attempts to book a slot from `T_start` to `T_end` in Zone X:
1. Calculate `timeUntilCheckin = T_start - Now`.
2. Determine current load:
   * **If `timeUntilCheckin` is close** (within `WALKIN_STAY_THRESHOLD_HOURS`):
     $$\text{Load} = \text{Active Walk-in Sessions (no booking linked)} + \text{Confirmed Bookings overlapping } [T_{start}, T_{end}]$$
   * **If `timeUntilCheckin` is far** (beyond `WALKIN_STAY_THRESHOLD_HOURS`):
     $$\text{Load} = \text{Confirmed Bookings overlapping } [T_{start}, T_{end}] \text{ only}$$
3. Deny booking if:
   $$\text{Load} + 1 > \text{Zone Capacity} \times \frac{\text{Zone.BookingLimitRate}}{100}$$

### 2.2 Auto Fallback at Check-in
When a booking checks in at slot A1:
1. If slot A1 is `Occupied` or bận (Maintenance, Blocked, etc.):
   1. Search for a fallback slot in the same Zone where:
      * Status is `Available`
      * There is no active session currently on the slot
      * There is no confirmed/pending booking on this slot overlapping the guest's booked window (including configured `BUFFER_TIME_MINUTES`).
   2. **If found**: Auto-reassign booking to the fallback slot, mark slot as `Occupied`, and proceed with check-in.
   3. **If not found**: Fail check-in gracefully and return detailed information to Staff (Occupying vehicle license plate, session ID, overdue duration) to resolve manually.

---

## 3. Predefined System Configurations

Defined in the `parking_system_configs` table:

| Key | Default Value | Description |
| :--- | :--- | :--- |
| `BUFFER_TIME_MINUTES` | `30` | Minutes of buffer/grace gap between consecutive bookings on the same slot. |
| `WALKIN_STAY_THRESHOLD_HOURS` | `2` | If check-in starts within this threshold from now, walk-in cars are included in the capacity load validation. |

---

## 4. Known Edge Cases (Deferred)

The following advanced scenarios have been deferred from the MVP but are recognized for future enhancements:

1. **Cross-Zone Fallback**
   * *Scenario*: A booking's slot is occupied, and no fallback slots are available within the same Zone.
   * *Resolution*: Search for alternative zones in the same building catering to the same vehicle type before returning an error.
2. **Concurrent Booking Race Conditions**
   * *Scenario*: Two clients submit bookings for the last available slot concurrently, both passing the capacity check before transactions commit.
   * *Resolution*: Implement optimistic concurrency control or row-level locking on the `zone` table during validation.
3. **Buffer Exhaustion Alerting**
   * *Scenario*: Fallbacks are frequently activated, indicating the zone is operating too close to capacity.
   * *Resolution*: Log a `BUFFER_ACTIVATED` event whenever a fallback triggers. Run a background worker to alert managers if events exceed a threshold.
4. **Walk-in Entry Restrictions**
   * *Scenario*: Walk-in cars enter a zone and consume slots designated as buffers for upcoming bookings.
   * *Resolution*: Integrate the barrier gate system with an API endpoint `GET /api/zones/{id}/is-accepting-walkin` to dynamically close barie when remaining free slots equal the required buffer.
5. **Departure Time Heuristics**
   * *Scenario*: A car is delayed, but the system doesn't know when it will check out.
   * *Resolution*: Apply a statistical model or ML heuristics on historical parking times to estimate the expected departure window.

---

## 5. Frontend Integration Guide

This guide outlines the required UI controls and error-handling steps for frontend applications.

### 5.1 Customer App (Create Booking Form)
* **Required Controls**: None (existing booking creation form remains unchanged).
* **API Action**: `POST /api/bookings`
* **Handling Errors**:
  * If the API returns the error code `ZONE_BOOKING_LIMIT_EXCEEDED`, intercept it and display a friendly message to the customer:
    > "This zone has reached its reservation capacity limit for the selected time window. Please select a different zone or adjust your arrival/departure time."

### 5.2 Staff Dashboard (Check-in Panel)
* **Required Controls**:
  * An alert modal that displays when a check-in fails due to capacity conflict.
  * Information fields to show who is occupying the slot and for how long.
* **API Action**: `POST /api/ParkingSession/check-in`
* **Handling Errors**:
  * If check-in fails with error code `NO_FALLBACK_SLOT_AVAILABLE`, the API will return details of the delayed vehicle.
  * **Action**: Display an alert modal showing the occupying vehicle's license plate and elapsed overdue minutes (e.g. `"License Plate: 51A-12345 (Delayed by 45 minutes)"`). Provide action buttons for the staff:
    * **[Call Driver]** - (Triggers driver contact flow).
    * **[Reassign Slot Manually]** - Opens a dropdown populated with other zones or manual slot selection (`OverrideSlotId`).

### 5.3 Manager Dashboard (System & Zone Configuration)
#### A. Zone Settings Form
* **Required Controls**:
  * A numeric input field for **Booking Limit Rate** (Range: `1` - `100`, Step: `1`, Unit: `%`).
* **API Action**: `PUT /api/zones/{id}` (Ensure this field is mapped to `BookingLimitRate`).

#### B. System Configuration Page
* **Required Controls**:
  * A configuration list/grid showing active keys.
  * Form inputs to update:
    * **Buffer Gap Time** (Key: `BUFFER_TIME_MINUTES`, input type: number)
    * **Walk-In Stay Threshold** (Key: `WALKIN_STAY_THRESHOLD_HOURS`, input type: number)
* **API Actions**:
  * `GET /api/ParkingSystemConfig` to list settings.
  * `PUT /api/ParkingSystemConfig` to upsert/update configuration values.

