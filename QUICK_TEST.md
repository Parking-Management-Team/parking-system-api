# ⚡ Quick Test Guide - API Ready to Go!

## 🚀 Start API (30 seconds)

```bash
cd parking-system-api
dotnet run --project src/PBMS.API
```

Wait for: `Application started. Press Ctrl+C to shut down.`

## 🌐 Open Swagger UI

**URL**: `http://localhost:5029/swagger`

---

## 🧪 Test These Endpoints in Swagger

### 1️⃣ Get All Vehicle Types
```
GET /api/vehicletype
```
✅ Should return 2 items: Xe Máy, Ô Tó

### 2️⃣ Get One Vehicle Type
```
GET /api/vehicletype/1
```
✅ Should return: Xe Máy

### 3️⃣ Create New Vehicle Type
```
POST /api/vehicletype

Body:
{
  "name": "Xe Tải"
}
```
✅ Should return new ID (likely 3+)

### 4️⃣ Update Vehicle Type
```
PUT /api/vehicletype/1

Body:
{
  "name": "Xe Máy (Updated)",
  "isActive": true
}
```
✅ Should return updated data

### 5️⃣ Delete Vehicle Type
```
DELETE /api/vehicletype/3
```
✅ Should return success message

---

## ✨ What's Different (No Migrations!)

✅ **No database setup needed**  
✅ **No connection strings to configure**  
✅ **No migrations to run**  
✅ **Auto-created in-memory database**  
✅ **Seed data loads automatically**  

---

## 🛠️ Technology Used

- **.NET 10.0** - Latest framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **Swagger** - Auto-documentation
- **In-Memory Database** - No external setup

---

## 📊 Files Changed

1. `src/PBMS.Infrastructure/DependencyInjection.cs` ✅ Modified
2. `src/PBMS.Infrastructure/PBMS.Infrastructure.csproj` ✅ Modified  
3. `src/PBMS.API/appsettings.Development.json` ✅ Created
4. `src/PBMS.API/Controllers/VehicleTypeController.cs` ✅ Modified

---

## 🎯 What You Get

✅ Fully functional REST API  
✅ Swagger UI documentation  
✅ CRUD operations for Vehicle Types  
✅ Error handling & validation  
✅ Seed data (Xe Máy, Ô Tó)  
✅ No external dependencies  

---

## ⚡ Done!

The API is ready to test. No migrations, no database setup needed!

**Open**: http://localhost:5029/swagger and start testing! 🎉
