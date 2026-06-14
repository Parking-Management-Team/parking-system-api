# Vehicle Type Management - Quick Start Guide

## Prerequisites
- .NET 10.0 SDK
- SQL Server (LocalDB or full version)
- Visual Studio or VS Code

## Setup Instructions

### 1. Configure Database Connection
Edit `src/PBMS.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PBMS_DB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 2. Create and Migrate Database
```bash
cd parking-system-api

# Create migrations (if needed)
dotnet ef    add CreateVehicleTypesTable --project src/PBMS.Infrastructure --startup-project src/PBMS.API

# Apply migrations
dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
```

### 3. Run the Application
```bash
dotnet run --project src/PBMS.API
```

The API will start at: `https://localhost:5001`

## API Endpoints Overview

### Get All Vehicle Types
```bash
curl -X GET "https://localhost:5001/api/vehicletype" \
  -H "Authorization: Bearer {YOUR_TOKEN}"
```

### Get Vehicle Type by ID
```bash
curl -X GET "https://localhost:5001/api/vehicletype/1" \
  -H "Authorization: Bearer {YOUR_TOKEN}"
```

### Create New Vehicle Type
```bash
curl -X POST "https://localhost:5001/api/vehicletype" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {YOUR_TOKEN}" \
  -d '{"name": "Xe Đạp"}'
```

### Update Vehicle Type
```bash
curl -X PUT "https://localhost:5001/api/vehicletype/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {YOUR_TOKEN}" \
  -d '{"name": "Xe Máy Lớn", "isActive": false}'
```

### Delete Vehicle Type
```bash
curl -X DELETE "https://localhost:5001/api/vehicletype/3" \
  -H "Authorization: Bearer {YOUR_TOKEN}"
```

## Default Data

After running migrations, these vehicle types are automatically created:
- ID 1: "Xe Máy" (Motorcycle)
- ID 2: "Ô Tó" (Car)

## Error Handling

All errors follow this format:
```json
{
  "success": false,
  "errorCode": "ERROR_CODE",
  "message": "Error message in Vietnamese",
  "data": null
}
```

Common error codes:
- `NOT_FOUND` - Vehicle type doesn't exist
- `NAME_EXISTS` - Name already exists
- `INVALID_NAME` - Name is empty
- `IN_USE_SESSIONS` - Used in parking sessions
- `IN_USE_BOOKINGS` - Used in bookings

## Project Structure

```
parking-system-api/
├── src/
│   ├── PBMS.Domain/
│   │   └── Entities/
│   │       ├── VehicleType.cs
│   │       ├── Vehicle.cs
│   │       └── ParkingSession.cs
│   ├── PBMS.Application/
│   │   └── Vehicle/
│   │       ├── DTOs/
│   │       ├── Interfaces/
│   │       └── Services/
│   ├── PBMS.Infrastructure/
│   │   ├── Repositories/
│   │   ├── Configurations/
│   │   ├── Migrations/
│   │   └── Data/
│   └── PBMS.API/
│       └── Controllers/
│           └── VehicleTypeController.cs
└── VEHICLE_TYPE_API.md
```

## Validation Rules

1. **Name Validation:**
   - Cannot be empty
   - Must be unique
   - Max 100 characters

2. **Deletion Constraints:**
   - Cannot delete if used in active parking sessions
   - Cannot delete if used in incomplete bookings

## Status Values

- **Active (true)**: Vehicle type is available for use
- **Inactive (false)**: Vehicle type is not available

## Response Format

All successful responses return:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* response data */ },
  "errorCode": null,
  "errors": null
}
```

## Logging

The application uses .NET logging. Check console output for:
- Information about received requests
- Errors and exceptions
- Operation details

## Development Notes

- **Framework**: .NET 10.0
- **Database**: SQL Server (via Entity Framework Core)
- **Architecture**: Clean Architecture (Domain → Application → Infrastructure → API)
- **Pattern**: Repository + Service layer pattern
- **Language**: English code, Vietnamese error messages

## Common Issues

### Database Connection Failed
- Check connection string in `appsettings.json`
- Ensure SQL Server is running
- Try: `(localdb)\\mssqllocaldb` for LocalDB

### Migration Errors
```bash
# Delete migration if needed
dotnet ef migrations remove --project src/PBMS.Infrastructure

# Re-apply migrations
dotnet ef database update --project src/PBMS.Infrastructure --startup-project src/PBMS.API
```

### Authorization Errors (401/403)
- Ensure authentication middleware is configured
- Check that token is properly passed in Authorization header
- Verify user has appropriate role (Manager/Admin)

## Support Files

- `VEHICLE_TYPE_API.md` - Complete API documentation
- `IMPLEMENTATION_SUMMARY.md` - Detailed implementation overview
- `README.md` - General project information

## Next Development Areas

- Implement pagination in GetAll endpoint
- Add sorting/filtering capabilities
- Implement audit logging
- Add batch operations support
- Create automated tests

---

For detailed API documentation, see: `VEHICLE_TYPE_API.md`
