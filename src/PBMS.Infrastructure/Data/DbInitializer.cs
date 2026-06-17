using Microsoft.EntityFrameworkCore;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using PBMS.Infrastructure.Data;
using BCrypt.Net;

namespace PBMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Seed Roles
        if (!await context.Set<Role>().AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { RoleName = "Admin", Description = "Hệ thống quản trị" },
                new Role { RoleName = "Manager", Description = "Quản lý bãi xe" },
                new Role { RoleName = "Staff", Description = "Nhân viên bãi xe" },
                new Role { RoleName = "Driver", Description = "Khách hàng gửi xe" }
            };
            await context.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed Vehicle Types
        if (!await context.Set<VehicleType>().AnyAsync())
        {
            var vTypes = new List<VehicleType>
            {
                new VehicleType { TypeName = "Xe máy", Description = "Xe gắn máy 2 bánh", VehicleTypeStatus = "Active" },
                new VehicleType { TypeName = "Ô tô", Description = "Xe hơi từ 4-7 chỗ", VehicleTypeStatus = "Active" }
            };
            await context.AddRangeAsync(vTypes);
            await context.SaveChangesAsync();
        }

        // 3. Seed/Update Accounts (Admin, Manager, Staff, Driver)
        var adminRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Admin");
        var managerRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Manager");
        var staffRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Staff");
        var driverRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Driver");

        // Admin
        var adminAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "admin");
        if (adminAccount == null)
        {
            adminAccount = new Account 
            { 
                Username = "admin", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"), 
                Email = "admin@pbms.com", 
                FullName = "System Admin",
                RoleId = adminRole!.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(adminAccount);
        }
        else
        {
            adminAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Update(adminAccount);
        }

        // Manager
        var managerAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "manager");
        if (managerAccount == null)
        {
            managerAccount = new Account 
            { 
                Username = "manager", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"), 
                Email = "manager@pbms.com", 
                FullName = "Trần Văn C (Quản lý)",
                RoleId = managerRole!.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(managerAccount);
        }
        else
        {
            managerAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Update(managerAccount);
        }

        // Staff
        var staffAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "staff");
        if (staffAccount == null)
        {
            staffAccount = new Account 
            { 
                Username = "staff", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"), 
                Email = "staff@pbms.com", 
                FullName = "Trần Thị B (Nhân viên)",
                RoleId = staffRole!.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(staffAccount);
        }
        else
        {
            staffAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Update(staffAccount);
        }

        // Driver
        var driverAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver");
        if (driverAccount == null)
        {
            driverAccount = new Account 
            { 
                Username = "driver", 
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"), 
                Email = "driver@pbms.com", 
                FullName = "Nguyễn Văn A (Tài xế)",
                RoleId = driverRole!.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(driverAccount);
        }
        else
        {
            driverAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Update(driverAccount);
        }

        await context.SaveChangesAsync();

        // 4. Seed Building, Floor, Zone, Slot
        if (!await context.Set<Building>().AnyAsync())
        {
            var building = new Building 
            { 
                Code = "BLD01", 
                Name = "Tòa nhà A", 
                Address = "Khu Công Nghệ Cao, Quận 9",
                TotalFloor = 2,
                Status = BuildingStatus.Active
            };
            await context.AddAsync(building);
            await context.SaveChangesAsync();

            var floor1 = new Floor { BuildingId = building.Id, FloorNumber = 1, Status = FloorStatus.Active };
            var floor2 = new Floor { BuildingId = building.Id, FloorNumber = 2, Status = FloorStatus.Active };
            await context.AddRangeAsync(floor1, floor2);
            await context.SaveChangesAsync();

            var motorType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Xe máy");
            var carType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Ô tô");

            var zoneMotor = new Zone 
            { 
                FloorId = floor1.Id, 
                Code = "ZM01", 
                Name = "Khu xe máy", 
                Capacity = 100, 
                VehicleTypeId = motorType!.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };
            var zoneCar = new Zone 
            { 
                FloorId = floor2.Id, 
                Code = "ZC01", 
                Name = "Khu ô tô", 
                Capacity = 10, 
                VehicleTypeId = carType!.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };
            await context.AddRangeAsync(zoneMotor, zoneCar);
            await context.SaveChangesAsync();

            // Seed Slots for Car Zone (Auto-generate 10 slots)
            for (int i = 1; i <= 10; i++)
            {
                context.Set<ParkingSlot>().Add(new ParkingSlot
                {
                    ZoneId = zoneCar.Id,
                    VehicleTypeId = carType.Id,
                    Code = $"ZC01-{i:D2}",
                    Name = $"Vị trí ZC01-{i:D2}",
                    Status = SlotStatus.Available
                });
            }
            await context.SaveChangesAsync();
        }

        // 5. Seed Cards
        if (!await context.Set<Card>().AnyAsync())
        {
            var cards = new List<Card>
            {
                new Card { CardCode = "CARD001", CardType = "PARKING_CARD", CardStatus = CardStatus.Available.ToString() },
                new Card { CardCode = "CARD002", CardType = "PARKING_CARD", CardStatus = CardStatus.Available.ToString() },
                new Card { CardCode = "CARD003", CardType = "PARKING_CARD", CardStatus = CardStatus.Available.ToString() }
            };
            await context.AddRangeAsync(cards);
            await context.SaveChangesAsync();
        }

        // 6. Seed Pricing Policies (Xe máy & Ô tô)
        if (!await context.Set<PricingPolicy>().AnyAsync())
        {
            var motorType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Xe máy");
            var carType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Ô tô");

            var policies = new List<PricingPolicy>
            {
                new PricingPolicy
                {
                    VehicleTypeId = motorType!.Id,
                    PolicyName = "Bảng giá vãng lai xe máy",
                    EffectiveStart = DateTime.UtcNow.AddHours(7).AddDays(-1), // GMT+7 yesterday
                    PricingPolicyStatus = "Active",
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "Khung giờ ngày",
                            StartTime = new TimeSpan(6, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 5000m,
                            IncrementBlockMinutes = 15,
                            IncrementPrice = 2000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        },
                        new PricingWindow
                        {
                            WindowName = "Khung giờ đêm",
                            StartTime = new TimeSpan(22, 0, 0),
                            EndTime = new TimeSpan(6, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 10000m,
                            IncrementBlockMinutes = 30,
                            IncrementPrice = 5000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        }
                    }
                },
                new PricingPolicy
                {
                    VehicleTypeId = carType!.Id,
                    PolicyName = "Bảng giá vãng lai ô tô",
                    EffectiveStart = DateTime.UtcNow.AddHours(7).AddDays(-1), // GMT+7 yesterday
                    PricingPolicyStatus = "Active",
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "Khung giờ ngày",
                            StartTime = new TimeSpan(6, 0, 0),
                            EndTime = new TimeSpan(22, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 20000m,
                            IncrementBlockMinutes = 15,
                            IncrementPrice = 5000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        },
                        new PricingWindow
                        {
                            WindowName = "Khung giờ đêm",
                            StartTime = new TimeSpan(22, 0, 0),
                            EndTime = new TimeSpan(6, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 40000m,
                            IncrementBlockMinutes = 30,
                            IncrementPrice = 10000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        }
                    }
                }
            };

            await context.AddRangeAsync(policies);
            await context.SaveChangesAsync();
        }

        // 7. Retrieve seeded accounts for vehicle and session relations
        staffAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "staff");
        driverAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver");

        // 8. Seed Vehicle for Driver
        var carTypeForSeed = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Ô tô");
        Vehicle? vehicle = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == "51G-12345");
        if (vehicle == null && driverAccount != null && carTypeForSeed != null)
        {
            vehicle = new Vehicle
            {
                AccountId = driverAccount.Id,
                VehicleTypeId = carTypeForSeed.Id,
                LicensePlate = "51G-12345",
                RegisteredDay = DateTime.UtcNow.AddHours(7),
                VehicleStatus = "ACTIVE"
            };
            await context.AddAsync(vehicle);
            await context.SaveChangesAsync();
        }

        // 9. Seed an ACTIVE Parking Session for testing checkout & VNPay payment
        if (vehicle != null)
        {
            var activeSession = await context.Set<ParkingSession>().FirstOrDefaultAsync(s => s.VehicleId == vehicle.Id && s.SessionStatus == "ACTIVE");
            if (activeSession == null)
            {
                var building = await context.Set<Building>().FirstOrDefaultAsync(b => b.Code == "BLD01");
                var card = await context.Set<Card>().FirstOrDefaultAsync(c => c.CardCode == "CARD001");
                var zone = await context.Set<Zone>().FirstOrDefaultAsync(z => z.Code == "ZC01");
                var slot = await context.Set<ParkingSlot>().FirstOrDefaultAsync(ps => ps.Code == "ZC01-01");

                if (building != null && card != null && zone != null && slot != null)
                {
                    // Update card status to Active
                    card.CardStatus = CardStatus.Active.ToString();

                    // Update slot status to Occupied
                    slot.Status = SlotStatus.Occupied;

                    activeSession = new ParkingSession
                    {
                        VehicleId = vehicle.Id,
                        BuildingId = building.Id,
                        CardId = card.Id,
                        ZoneId = zone.Id,
                        SlotId = slot.Id,
                        InStaffId = staffAccount?.Id,
                        CheckInTime = DateTime.UtcNow.AddHours(7).AddHours(-2), // 2 hours ago (so fee is positive)
                        LicensePlateIn = "51G-12345",
                        SessionStatus = "ACTIVE"
                    };

                    await context.AddAsync(activeSession);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
