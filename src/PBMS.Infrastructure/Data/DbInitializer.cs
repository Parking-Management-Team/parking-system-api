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
                new Role { RoleName = "Admin", Description = "System Administrator" },
                new Role { RoleName = "Manager", Description = "Parking Lot Manager" },
                new Role { RoleName = "Staff", Description = "Parking Lot Staff" },
                new Role { RoleName = "Driver", Description = "Vehicle Driver" }
            };
            await context.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }

        // 2. Seed Vehicle Types
        var motorcycleType = await context.Set<VehicleType>()
            .FirstOrDefaultAsync(v => v.TypeName == "Motorcycle" || v.VehicleTypeCode == "MOTOR");
        if (motorcycleType == null)
        {
            motorcycleType = new VehicleType { TypeName = "Motorcycle", VehicleTypeCode = "MOTOR", Description = "2-wheel motorcycle", VehicleTypeStatus = "Active" };
            await context.AddAsync(motorcycleType);
            await context.SaveChangesAsync();
        }

        var carType = await context.Set<VehicleType>()
            .FirstOrDefaultAsync(v => v.TypeName == "Car" || v.VehicleTypeCode == "CAR");
        if (carType == null)
        {
            carType = new VehicleType { TypeName = "Car", VehicleTypeCode = "CAR", Description = "4-7 seat passenger car", VehicleTypeStatus = "Active" };
            await context.AddAsync(carType);
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
                FullName = "John Doe (Manager)",
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
                FullName = "Jane Smith (Staff)",
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
                FullName = "Bob Johnson (Driver)",
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
                Name = "Building A", 
                Address = "High Tech Park, District 9",
                TotalFloor = 2,
                Status = BuildingStatus.Active
            };
            await context.AddAsync(building);
            await context.SaveChangesAsync();

            var floor1 = new Floor { BuildingId = building.Id, FloorNumber = 1, Status = FloorStatus.Active };
            var floor2 = new Floor { BuildingId = building.Id, FloorNumber = 2, Status = FloorStatus.Active };
            await context.AddRangeAsync(floor1, floor2);
            await context.SaveChangesAsync();

            motorcycleType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Motorcycle");
            carType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car");

            var zoneMotor = new Zone 
            { 
                FloorId = floor1.Id, 
                Code = "ZM01", 
                Name = "Motorbike Zone", 
                Capacity = 100, 
                VehicleTypeId = motorcycleType!.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };
            var zoneCar = new Zone 
            { 
                FloorId = floor2.Id, 
                Code = "ZC01", 
                Name = "Car Zone", 
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
                    Name = $"Slot ZC01-{i:D2}",
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

        // 6. Seed Pricing Policies (Motorcycle & Car)
        if (!await context.Set<PricingPolicy>().AnyAsync())
        {
            motorcycleType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Motorcycle");
            carType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car");

            var policies = new List<PricingPolicy>
            {
                new PricingPolicy
                {
                    VehicleTypeId = motorcycleType!.Id,
                    PolicyName = "Motorbike Casual Pricing",
                    EffectiveStart = DateTime.UtcNow.AddHours(7).AddDays(-1), // GMT+7 yesterday
                    PricingPolicyStatus = "Active",
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "Day Time Window",
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
                            WindowName = "Night Time Window",
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
                    PolicyName = "Car Casual Pricing",
                    EffectiveStart = DateTime.UtcNow.AddHours(7).AddDays(-1), // GMT+7 yesterday
                    PricingPolicyStatus = "Active",
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "Day Time Window",
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
                            WindowName = "Night Time Window",
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
        var carTypeForSeed = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car");
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
        // 10. Seed Subscription Price Configs
        if (!await context.Set<SubscriptionPriceConfig>().AnyAsync())
        {
            var motorcycleConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = motorcycleType!.Id,
                Price = 150000, // 150k/tháng
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            var carConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = carType!.Id,
                Price = 1000000, // 1 triệu/tháng
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            await context.AddRangeAsync(motorcycleConfig, carConfig);
            await context.SaveChangesAsync();
        }

        // 11. Seed IncidentTypes
        if (!await context.Set<IncidentType>().AnyAsync())
        {
            var lostCardType = new IncidentType
            {
                IncidentCode = "LOST_CARD",
                IncidentName = "Mất thẻ gửi xe",
                Description = "Sự cố mất thẻ đỗ xe vật lý"
            };
            var crashType = new IncidentType
            {
                IncidentCode = "VEHICLE_CRASH",
                IncidentName = "Va chạm xe",
                Description = "Sự cố va chạm hoặc gây tai nạn trong bãi xe"
            };
            var wrongLaneType = new IncidentType
            {
                IncidentCode = "WRONG_LANE",
                IncidentName = "Đi sai làn đường",
                Description = "Sự cố đi sai làn đường quy định"
            };

            await context.AddRangeAsync(lostCardType, crashType, wrongLaneType);
            await context.SaveChangesAsync();
        }

        // 12. Seed Penalty Configs based on existing IncidentTypes
        var incidentTypes = await context.Set<IncidentType>().ToListAsync();
        if (incidentTypes.Any() && !await context.Set<PenaltyConfig>().AnyAsync())
        {
            var penaltyConfigs = new List<PenaltyConfig>();
            foreach (var it in incidentTypes)
            {
                decimal fee = 50000; // default penalty
                if (it.IncidentCode.Equals("LOST_CARD", StringComparison.OrdinalIgnoreCase))
                {
                    fee = 100000; // Phạt mất thẻ: 100k
                }
                else if (it.IncidentCode.Equals("VEHICLE_CRASH", StringComparison.OrdinalIgnoreCase))
                {
                    fee = 200000; // Phạt va chạm xe: 200k
                }
                else if (it.IncidentCode.Equals("WRONG_LANE", StringComparison.OrdinalIgnoreCase))
                {
                    fee = 30000; // Phạt đi sai làn: 30k
                }

                penaltyConfigs.Add(new PenaltyConfig
                {
                    IncidentTypeId = it.Id,
                    PenaltyFee = fee,
                    EffectiveFrom = DateTime.UtcNow,
                    IsActive = true
                });
            }

            await context.AddRangeAsync(penaltyConfigs);
            await context.SaveChangesAsync();
        }
    }
}
