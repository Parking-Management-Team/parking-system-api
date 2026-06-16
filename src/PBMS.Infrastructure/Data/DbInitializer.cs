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

        // 3. Seed Accounts
        if (!await context.Set<Account>().AnyAsync())
        {
            var adminRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Admin");
            var accounts = new List<Account>
            {
                new Account 
                { 
                    Username = "admin", 
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), 
                    Email = "admin@pbms.com", 
                    FullName = "System Admin",
                    RoleId = adminRole!.Id,
                    AccountStatus = "Active"
                }
            };
            await context.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }

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
    }
}
