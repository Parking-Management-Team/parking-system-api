using Microsoft.EntityFrameworkCore;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, bool resetPasswords = false)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
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
            Console.WriteLine($"[DIAG] Roles section finished. Role count: {await context.Set<Role>().CountAsync()}");
        }

        // 2. Seed Vehicle Types
        var motorcycleType = await context.Set<VehicleType>()
            .FirstOrDefaultAsync(v => v.TypeName == "Motorcycle" || v.VehicleTypeCode == "MOTOR");
        if (motorcycleType == null)
        {
            motorcycleType = new VehicleType { TypeName = "Motorcycle", VehicleTypeCode = "MOTOR", Description = "2-wheel motorcycle", VehicleTypeStatus = "Active", BufferRatio = 5 };
            await context.AddAsync(motorcycleType);
            await context.SaveChangesAsync();
        }

        var carType = await context.Set<VehicleType>()
            .FirstOrDefaultAsync(v => v.TypeName == "Car" || v.VehicleTypeCode == "CAR");
        if (carType == null)
        {
            carType = new VehicleType { TypeName = "Car", VehicleTypeCode = "CAR", Description = "4-7 seat passenger car", VehicleTypeStatus = "Active", BufferRatio = 15 };
            await context.AddAsync(carType);
            await context.SaveChangesAsync();
        }
        Console.WriteLine($"[DIAG] VehicleTypes section finished. VehicleType count: {await context.Set<VehicleType>().CountAsync()}");

        // 3. Seed/Update Accounts (Admin, Manager, Staff, Driver)
        var adminRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Admin")
            ?? throw new InvalidOperationException("Required role 'Admin' was not found in the database. Please seed roles first.");
        var managerRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Manager")
            ?? throw new InvalidOperationException("Required role 'Manager' was not found in the database. Please seed roles first.");
        var staffRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Staff")
            ?? throw new InvalidOperationException("Required role 'Staff' was not found in the database. Please seed roles first.");
        var driverRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Driver")
            ?? throw new InvalidOperationException("Required role 'Driver' was not found in the database. Please seed roles first.");

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
                RoleId = adminRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(adminAccount);
        }
        else if (resetPasswords)
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
                RoleId = managerRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(managerAccount);
        }
        else if (resetPasswords)
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
                RoleId = staffRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(staffAccount);
        }
        else if (resetPasswords)
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
                RoleId = driverRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(driverAccount);
        }
        else if (resetPasswords)
        {
            driverAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123");
            context.Update(driverAccount);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[DIAG] Accounts section finished. Account count: {await context.Set<Account>().CountAsync()}");

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

            // Floor 1 has Motorcycle Zone (ZM01 - 25 slots) and Car Zone (ZC01 - 25 slots)
            var zoneMotorF1 = new Zone 
            { 
                FloorId = floor1.Id, 
                Code = "ZM01", 
                Name = "Motorbike Zone F1", 
                Capacity = 25, 
                VehicleTypeId = motorcycleType!.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };
            var zoneCarF1 = new Zone 
            { 
                FloorId = floor1.Id, 
                Code = "ZC01", 
                Name = "Car Zone F1", 
                Capacity = 25, 
                VehicleTypeId = carType!.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };

            // Floor 2 has Car Zone (ZC02 - 25 slots)
            var zoneCarF2 = new Zone 
            { 
                FloorId = floor2.Id, 
                Code = "ZC02", 
                Name = "Car Zone F2", 
                Capacity = 25, 
                VehicleTypeId = carType.Id,
                AccessType = ZoneAccessType.General,
                Status = ZoneStatus.Available
            };

            await context.AddRangeAsync(zoneMotorF1, zoneCarF1, zoneCarF2);
            await context.SaveChangesAsync();

            // Seed Slots for Floor 1 - Motorbike Zone (25 slots)
            for (int i = 1; i <= 25; i++)
            {
                context.Set<ParkingSlot>().Add(new ParkingSlot
                {
                    ZoneId = zoneMotorF1.Id,
                    VehicleTypeId = motorcycleType.Id,
                    Code = $"ZM01-{i:D2}",
                    Name = $"Slot ZM01-{i:D2}",
                    Status = SlotStatus.Available
                });
            }

            // Seed Slots for Floor 1 - Car Zone (25 slots)
            for (int i = 1; i <= 25; i++)
            {
                context.Set<ParkingSlot>().Add(new ParkingSlot
                {
                    ZoneId = zoneCarF1.Id,
                    VehicleTypeId = carType.Id,
                    Code = $"ZC01-{i:D2}",
                    Name = $"Slot ZC01-{i:D2}",
                    Status = SlotStatus.Available
                });
            }

            // Seed Slots for Floor 2 - Car Zone (25 slots)
            for (int i = 1; i <= 25; i++)
            {
                context.Set<ParkingSlot>().Add(new ParkingSlot
                {
                    ZoneId = zoneCarF2.Id,
                    VehicleTypeId = carType.Id,
                    Code = $"ZC02-{i:D2}",
                    Name = $"Slot ZC02-{i:D2}",
                    Status = SlotStatus.Available
                });
            }

            await context.SaveChangesAsync();
        }

        // 5. Seed Cards (Delete mock and add 50 physical cards CARD001 -> CARD050)
        if (!await context.Set<Card>().AnyAsync())
        {
            var cards = new List<Card>();
            for (int i = 1; i <= 50; i++)
            {
                cards.Add(new Card 
                { 
                    CardCode = $"CARD{i:D3}", 
                    CardType = "PARKING_CARD", 
                    CardStatus = CardStatus.Available.ToString() 
                });
            }
            await context.AddRangeAsync(cards);
            await context.SaveChangesAsync();
        }

        // 6. Seed Pricing Policies (Motorcycle & Car)
        if (!await context.Set<PricingPolicy>().AnyAsync())
        {
            var motorcycleTypeForPolicy = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Motorcycle")
                ?? throw new InvalidOperationException("Required VehicleType 'Motorcycle' was not found in the database. Please seed vehicle types first.");
            var carTypeForPolicy = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car")
                ?? throw new InvalidOperationException("Required VehicleType 'Car' was not found in the database. Please seed vehicle types first.");

            var today = DateTime.UtcNow.AddHours(7).Date;

            var policies = new List<PricingPolicy>
            {
                // Motorcycle Policy (Default, Priority = 0)
                new PricingPolicy
                {
                    VehicleTypeId = motorcycleTypeForPolicy.Id,
                    PolicyName = "Motorbike Casual Pricing",
                    EffectiveStart = today.AddDays(-10),
                    PricingPolicyStatus = "Active",
                    Priority = 0,
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
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod", ExecutionOrder = 1, IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing", ExecutionOrder = 2, IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 5000m }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing", ExecutionOrder = 3, IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 15, IncrementPriceAmount = 2000m, ThresholdPercentage = 50 }
                        },
                        new PricingRule
                        {
                            RuleType = "DailyCap", ExecutionOrder = 4, IsActive = true,
                            DailyCapRuleConfig = new DailyCapRuleConfig { MaximumDailyAmount = 50000m }
                        }
                    }
                },
                
                // Car Policy 1: Old Expired Policy (Priority = 0)
                new PricingPolicy
                {
                    VehicleTypeId = carTypeForPolicy.Id,
                    PolicyName = "Car Old Pricing (Expired)",
                    EffectiveStart = today.AddDays(-30),
                    EffectiveEnd = today.AddDays(-10).AddSeconds(-1), // Ended before default starts
                    PricingPolicyStatus = "Expired",
                    Priority = 0,
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "24H Window",
                            StartTime = new TimeSpan(0, 0, 0),
                            EndTime = new TimeSpan(24, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 10000m,
                            IncrementBlockMinutes = 60,
                            IncrementPrice = 5000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        }
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod", ExecutionOrder = 1, IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing", ExecutionOrder = 2, IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 10000m }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing", ExecutionOrder = 3, IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 5000m, ThresholdPercentage = 50 }
                        }
                    }
                },

                // Car Policy 2: Active Default Policy (Priority = 0)
                new PricingPolicy
                {
                    VehicleTypeId = carTypeForPolicy.Id,
                    PolicyName = "Car Casual Pricing (Default)",
                    EffectiveStart = today.AddDays(-10),
                    EffectiveEnd = null,
                    PricingPolicyStatus = "Active",
                    Priority = 0,
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
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod", ExecutionOrder = 1, IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing", ExecutionOrder = 2, IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 20000m }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing", ExecutionOrder = 3, IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 15, IncrementPriceAmount = 5000m, ThresholdPercentage = 50 }
                        },
                        new PricingRule
                        {
                            RuleType = "DailyCap", ExecutionOrder = 4, IsActive = true,
                            DailyCapRuleConfig = new DailyCapRuleConfig { MaximumDailyAmount = 150000m }
                        }
                    }
                },

                // Car Policy 3: Active Holiday Policy (Priority = 1, overlapping default)
                new PricingPolicy
                {
                    VehicleTypeId = carTypeForPolicy.Id,
                    PolicyName = "Car Holiday Special Pricing",
                    EffectiveStart = today.AddDays(-2),
                    EffectiveEnd = today.AddDays(2),
                    PricingPolicyStatus = "Active",
                    Priority = 1, // Higher priority overrides default during holiday range
                    PricingWindows = new List<PricingWindow>
                    {
                        new PricingWindow
                        {
                            WindowName = "24H Holiday Window",
                            StartTime = new TimeSpan(0, 0, 0),
                            EndTime = new TimeSpan(24, 0, 0),
                            BaseDurationMinutes = 60,
                            BasePrice = 40000m,
                            IncrementBlockMinutes = 60,
                            IncrementPrice = 20000m,
                            WindowCap = null,
                            GracePeriodMinutes = 0
                        }
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod", ExecutionOrder = 1, IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing", ExecutionOrder = 2, IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig { BaseDurationMinutes = 60, BasePriceAmount = 40000m }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing", ExecutionOrder = 3, IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig { IncrementIntervalMinutes = 60, IncrementPriceAmount = 20000m, ThresholdPercentage = 50 }
                        },
                        new PricingRule
                        {
                            RuleType = "DailyCap", ExecutionOrder = 4, IsActive = true,
                            DailyCapRuleConfig = new DailyCapRuleConfig { MaximumDailyAmount = 300000m }
                        }
                    }
                }
            };

            await context.AddRangeAsync(policies);
            await context.SaveChangesAsync();
        }

        // 7. Seed Vehicle for Driver
        driverAccount = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver");
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

        // 8. Active Parking Session - REMOVED completely to avoid active mock parking session seeding

        // 9. Seed Subscription Price Configs
        if (!await context.Set<SubscriptionPriceConfig>().AnyAsync())
        {
            var motorcycleTypeForConfig = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Motorcycle")
                ?? throw new InvalidOperationException("Required VehicleType 'Motorcycle' was not found in the database. Please seed vehicle types first.");
            var carTypeForConfig = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car")
                ?? throw new InvalidOperationException("Required VehicleType 'Car' was not found in the database. Please seed vehicle types first.");

            var motorcycleConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = motorcycleTypeForConfig.Id,
                Price = 150000, // 150k/tháng
                DurationDays = 30,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            var carConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = carTypeForConfig.Id,
                Price = 1000000, // 1 triệu/tháng
                DurationDays = 30,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            await context.AddRangeAsync(motorcycleConfig, carConfig);
            await context.SaveChangesAsync();
        }

        // 10. Seed IncidentTypes
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
            var lateCheckoutType = new IncidentType
            {
                IncidentCode = "LATE_CHECKOUT",
                IncidentName = "Đỗ xe quá giờ",
                Description = "Sự cố đỗ xe quá thời gian đặt chỗ quy định"
            };

            await context.AddRangeAsync(lostCardType, crashType, wrongLaneType, lateCheckoutType);
            await context.SaveChangesAsync();
        }

        // 11. Seed Penalty Configs based on existing IncidentTypes
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
                else if (it.IncidentCode.Equals("LATE_CHECKOUT", StringComparison.OrdinalIgnoreCase))
                {
                    fee = 50000; // Phạt quá giờ: 50k
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

        // 12. Seed Demo Bookings/Sessions if none exist
        if (!await context.Set<Booking>().AnyAsync())
        {
            await SeedDemoDataInternalAsync(context);
        }

        await transaction.CommitAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--> Error seeding database: {ex.Message}");
        await transaction.RollbackAsync();
        throw;
    }
}

    public static async Task ResetDemoDataAsync(AppDbContext context)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            // 1. Wipe time-sensitive transactional tables
            context.Set<Payment>().RemoveRange(await context.Set<Payment>().ToListAsync());
            context.Set<Incident>().RemoveRange(await context.Set<Incident>().ToListAsync());
            context.Set<ParkingSession>().RemoveRange(await context.Set<ParkingSession>().ToListAsync());
            context.Set<Booking>().RemoveRange(await context.Set<Booking>().ToListAsync());
            await context.SaveChangesAsync();

            // 2. Reset slot statuses to Available
            var slots = await context.Set<ParkingSlot>().ToListAsync();
            foreach (var slot in slots)
            {
                slot.Status = SlotStatus.Available;
            }
            context.UpdateRange(slots);

            // 3. Reset card statuses to Available
            var cards = await context.Set<Card>().ToListAsync();
            foreach (var card in cards)
            {
                card.CardStatus = CardStatus.Available.ToString();
                card.LostAt = null;
            }
            context.UpdateRange(cards);
            await context.SaveChangesAsync();

            // 4. Seed the fresh relative demo data
            await SeedDemoDataInternalAsync(context);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task SeedDemoDataInternalAsync(AppDbContext context)
    {
        var driverRole = await context.Set<Role>().FirstOrDefaultAsync(r => r.RoleName == "Driver")
            ?? throw new InvalidOperationException("Required role 'Driver' was not found in the database.");
        var carType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Car" || v.VehicleTypeCode == "CAR")
            ?? throw new InvalidOperationException("Required VehicleType 'Car' was not found in the database.");
        var motorcycleType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.TypeName == "Motorcycle" || v.VehicleTypeCode == "MOTOR")
            ?? throw new InvalidOperationException("Required VehicleType 'Motorcycle' was not found in the database.");
        var staff = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "staff")
            ?? throw new InvalidOperationException("Required staff account was not found.");
        var testDriver = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver")
            ?? throw new InvalidOperationException("Required default driver account was not found.");
        var building = await context.Set<Building>().FirstOrDefaultAsync(b => b.Code == "BLD01")
            ?? throw new InvalidOperationException("Required building 'BLD01' was not found.");

        // Zones
        var zoneMotorF1 = await context.Set<Zone>().FirstOrDefaultAsync(z => z.Code == "ZM01")
            ?? throw new InvalidOperationException("Required zone 'ZM01' was not found.");
        var zoneCarF1 = await context.Set<Zone>().FirstOrDefaultAsync(z => z.Code == "ZC01")
            ?? throw new InvalidOperationException("Required zone 'ZC01' was not found.");
        var zoneCarF2 = await context.Set<Zone>().FirstOrDefaultAsync(z => z.Code == "ZC02")
            ?? throw new InvalidOperationException("Required zone 'ZC02' was not found.");

        // Slots
        var slotsF1 = await context.Set<ParkingSlot>().Where(s => s.ZoneId == zoneCarF1.Id).ToListAsync();
        var slotsMotor = await context.Set<ParkingSlot>().Where(s => s.ZoneId == zoneMotorF1.Id).ToListAsync();
        var slotsF2 = await context.Set<ParkingSlot>().Where(s => s.ZoneId == zoneCarF2.Id).ToListAsync();

        var cards = await context.Set<Card>().ToListAsync();
        var card1 = cards.FirstOrDefault(c => c.CardCode == "CARD001") ?? cards[0];
        var card2 = cards.FirstOrDefault(c => c.CardCode == "CARD002") ?? cards[1];
        var card3 = cards.FirstOrDefault(c => c.CardCode == "CARD003") ?? cards[2];

        // Seed additional driver accounts if not present
        var driver2 = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver2");
        if (driver2 == null)
        {
            driver2 = new Account
            {
                Username = "driver2",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Email = "driver2@pbms.com",
                FullName = "Alice Smith (Demo Driver 2)",
                RoleId = driverRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(driver2);
            await context.SaveChangesAsync();
        }

        var driver3 = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver3");
        if (driver3 == null)
        {
            driver3 = new Account
            {
                Username = "driver3",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                Email = "driver3@pbms.com",
                FullName = "Charlie Brown (Demo Driver 3)",
                RoleId = driverRole.Id,
                AccountStatus = "Active"
            };
            await context.AddAsync(driver3);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // SCENARIO 1: Blacklisted Vehicle & Driver Account Demo (51A-999.99)
        // =========================================================================
        var blacklistedVehicle = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == "51A-999.99");
        if (blacklistedVehicle == null)
        {
            blacklistedVehicle = new Vehicle
            {
                AccountId = testDriver.Id,
                VehicleTypeId = carType.Id,
                LicensePlate = "51A-999.99",
                RegisteredDay = DateTime.UtcNow.AddDays(-40),
                VehicleStatus = "ACTIVE"
            };
            await context.AddAsync(blacklistedVehicle);
            await context.SaveChangesAsync();
        }

        var blacklistEntry = await context.Set<Blacklist>().FirstOrDefaultAsync(b => b.VehicleId == blacklistedVehicle.Id);
        if (blacklistEntry == null)
        {
            blacklistEntry = new Blacklist
            {
                VehicleId = blacklistedVehicle.Id,
                Reason = "Repeated unpaid parking fees and unauthorized parking behavior.",
                IsDeleted = false
            };
            await context.AddAsync(blacklistEntry);
            await context.SaveChangesAsync();
        }

        // =========================================================================
        // SCENARIO 2: 2 Motorcycles + 3 Cars Completed Checkout (COMPLETED)
        // =========================================================================

        // 2A. Motorcycle 1 (Walk-in / Checkout Thường): 59T1-111.11
        var motor1 = await GetOrCreateVehicleAsync(context, testDriver.Id, motorcycleType.Id, "59T1-111.11");
        var motorSession1 = new ParkingSession
        {
            VehicleId = motor1.Id,
            BuildingId = building.Id,
            CardId = card1.Id,
            ZoneId = zoneMotorF1.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-6),
            CheckOutTime = DateTime.UtcNow.AddHours(-3),
            LicensePlateIn = motor1.LicensePlate,
            LicensePlateOut = motor1.LicensePlate,
            SessionStatus = SessionStatus.Completed,
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(motorSession1);
        await context.SaveChangesAsync();

        var paymentMotor1 = new Payment
        {
            SessionId = motorSession1.Id,
            Amount = 5000m,
            PaymentMethod = "CASH",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-3).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-3)
        };
        await context.AddAsync(paymentMotor1);

        // 2B. Motorcycle 2 (Booking / Checkout Booking): 59T1-222.22
        var motor2 = await GetOrCreateVehicleAsync(context, driver2.Id, motorcycleType.Id, "59T1-222.22");
        var motorBooking2 = new Booking
        {
            AccountId = driver2.Id,
            VehicleId = motor2.Id,
            VehicleTypeId = motorcycleType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(-5),
            PlannedCheckoutTime = DateTime.UtcNow.AddHours(-3),
            DepositAmount = 13000m,
            BookingStatus = BookingStatus.CheckedIn,
            PaymentDeadline = DateTime.UtcNow.AddHours(-5).AddMinutes(-15),
            CheckinGraceUntil = DateTime.UtcNow.AddHours(-5).AddMinutes(30)
        };
        await context.AddAsync(motorBooking2);
        await context.SaveChangesAsync();

        var motorSession2 = new ParkingSession
        {
            VehicleId = motor2.Id,
            BuildingId = building.Id,
            CardId = card2.Id,
            ZoneId = zoneMotorF1.Id,
            BookingId = motorBooking2.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-5),
            CheckOutTime = DateTime.UtcNow.AddHours(-3),
            LicensePlateIn = motor2.LicensePlate,
            LicensePlateOut = motor2.LicensePlate,
            SessionStatus = SessionStatus.Completed,
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(motorSession2);
        await context.SaveChangesAsync();

        var paymentMotor2 = new Payment
        {
            BookingId = motorBooking2.Id,
            Amount = 13000m,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-5).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-5)
        };
        await context.AddAsync(paymentMotor2);

        // 2C. Car 1 (Walk-in / Checkout Thường - Demo tính giá 40k): 30H-999.99
        var car1 = await GetOrCreateVehicleAsync(context, testDriver.Id, carType.Id, "30H-999.99");
        var slotCar1 = slotsF1.FirstOrDefault(s => s.Code == "ZC01-01") ?? slotsF1[0];
        var carSession1 = new ParkingSession
        {
            VehicleId = car1.Id,
            BuildingId = building.Id,
            CardId = card2.Id,
            ZoneId = zoneCarF1.Id,
            SlotId = slotCar1.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-4),
            CheckOutTime = DateTime.UtcNow.AddHours(-0.5), // 3.5h stay
            LicensePlateIn = car1.LicensePlate,
            LicensePlateOut = car1.LicensePlate,
            SessionStatus = SessionStatus.Completed,
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(carSession1);
        await context.SaveChangesAsync();

        var paymentCar1 = new Payment
        {
            SessionId = carSession1.Id,
            Amount = 40000m,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-0.5).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-0.5)
        };
        await context.AddAsync(paymentCar1);

        // 2D. Car 2 (Booking / Checkout Booking): 51G-67890
        var car2 = await GetOrCreateVehicleAsync(context, driver2.Id, carType.Id, "51G-67890");
        var slotCar2 = slotsF1.FirstOrDefault(s => s.Code == "ZC01-02") ?? slotsF1[1];
        var carBooking2 = new Booking
        {
            AccountId = driver2.Id,
            VehicleId = car2.Id,
            VehicleTypeId = carType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(-5),
            PlannedCheckoutTime = DateTime.UtcNow.AddHours(-1),
            DepositAmount = 80000m,
            BookingStatus = BookingStatus.CheckedIn,
            PaymentDeadline = DateTime.UtcNow.AddHours(-5).AddMinutes(-15),
            CheckinGraceUntil = DateTime.UtcNow.AddHours(-5).AddMinutes(30),
            SlotId = slotCar2.Id
        };
        await context.AddAsync(carBooking2);
        await context.SaveChangesAsync();

        var carSession2 = new ParkingSession
        {
            VehicleId = car2.Id,
            BuildingId = building.Id,
            CardId = card1.Id,
            ZoneId = zoneCarF1.Id,
            SlotId = slotCar2.Id,
            BookingId = carBooking2.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-5),
            CheckOutTime = DateTime.UtcNow.AddHours(-1),
            LicensePlateIn = car2.LicensePlate,
            LicensePlateOut = car2.LicensePlate,
            SessionStatus = SessionStatus.Completed,
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(carSession2);
        await context.SaveChangesAsync();

        var paymentCar2 = new Payment
        {
            BookingId = carBooking2.Id,
            Amount = 80000m,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-5).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-5)
        };
        await context.AddAsync(paymentCar2);

        // 2E. Car 3 (Walk-in / Checkout Thường): 51H-333.33
        var car3 = await GetOrCreateVehicleAsync(context, driver3.Id, carType.Id, "51H-333.33");
        var carSession3 = new ParkingSession
        {
            VehicleId = car3.Id,
            BuildingId = building.Id,
            CardId = card3.Id,
            ZoneId = zoneCarF1.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-7),
            CheckOutTime = DateTime.UtcNow.AddHours(-2), // 5h stay
            LicensePlateIn = car3.LicensePlate,
            LicensePlateOut = car3.LicensePlate,
            SessionStatus = SessionStatus.Completed,
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(carSession3);
        await context.SaveChangesAsync();

        var paymentCar3 = new Payment
        {
            SessionId = carSession3.Id,
            Amount = 60000m,
            PaymentMethod = "CASH",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-2).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-2)
        };
        await context.AddAsync(paymentCar3);

        // =========================================================================
        // SCENARIO 3: 1 Late Booking (Checked in 1 hour late / Planned 1 hour ago) (51G-888.88)
        // =========================================================================
        var lateCar = await GetOrCreateVehicleAsync(context, driver2.Id, carType.Id, "51G-888.88");
        var slotCar4 = slotsF1.FirstOrDefault(s => s.Code == "ZC01-04") ?? slotsF1[3];
        var lateBooking = new Booking
        {
            AccountId = driver2.Id,
            VehicleId = lateCar.Id,
            VehicleTypeId = carType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(-1), // Planned 1h ago
            PlannedCheckoutTime = DateTime.UtcNow.AddHours(3),
            DepositAmount = 80000m,
            BookingStatus = BookingStatus.Confirmed,
            PaymentDeadline = DateTime.UtcNow.AddHours(-1).AddMinutes(-15),
            CheckinGraceUntil = DateTime.UtcNow.AddHours(-1).AddMinutes(30),
            SlotId = slotCar4.Id
        };
        await context.AddAsync(lateBooking);
        await context.SaveChangesAsync();

        var paymentLateBooking = new Payment
        {
            BookingId = lateBooking.Id,
            Amount = 80000m,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddHours(-1).Ticks,
            PaymentTime = DateTime.UtcNow.AddHours(-1).AddMinutes(-10)
        };
        await context.AddAsync(paymentLateBooking);

        // =========================================================================
        // SCENARIO 4: 1 Overnight Active Car Session (Checked in 24 hours ago) (51H-777.77)
        // =========================================================================
        var overnightCar = await GetOrCreateVehicleAsync(context, driver3.Id, carType.Id, "51H-777.77");
        var slotCar3 = slotsF1.FirstOrDefault(s => s.Code == "ZC01-03") ?? slotsF1[2];
        var overnightSession = new ParkingSession
        {
            VehicleId = overnightCar.Id,
            BuildingId = building.Id,
            CardId = card3.Id,
            ZoneId = zoneCarF1.Id,
            SlotId = slotCar3.Id,
            CheckInTime = DateTime.UtcNow.AddDays(-1),
            CheckOutTime = null,
            LicensePlateIn = overnightCar.LicensePlate,
            SessionStatus = SessionStatus.Active,
            InStaffId = staff.Id
        };
        await context.AddAsync(overnightSession);
        await context.SaveChangesAsync();

        card3.CardStatus = CardStatus.Active.ToString();
        context.Update(card3);
        slotCar3.Status = SlotStatus.Occupied;
        context.Update(slotCar3);

        // =========================================================================
        // SCENARIO 5: Fill Booking 80% Capacity for Floor 2 Zone (ZC02)
        // Zone ZC02 Capacity = 25, 80% = 20 Confirmed Bookings (51K-000.01 to 51K-000.20)
        // =========================================================================
        var zc02Bookings = new List<Booking>();
        var zc02Payments = new List<Payment>();

        var futureStart = DateTime.UtcNow.AddHours(2);
        var futureEnd = DateTime.UtcNow.AddHours(6);

        int countToFill = 20; // 80% of 25 slots
        for (int i = 1; i <= countToFill; i++)
        {
            var plate = $"51K-000.{i:D2}";
            var fillVehicle = await GetOrCreateVehicleAsync(context, driver2.Id, carType.Id, plate);
            var fillSlot = slotsF2.FirstOrDefault(s => s.Code == $"ZC02-{i:D2}") ?? (i - 1 < slotsF2.Count ? slotsF2[i - 1] : null);

            var fillBooking = new Booking
            {
                AccountId = driver2.Id,
                VehicleId = fillVehicle.Id,
                VehicleTypeId = carType.Id,
                BuildingId = building.Id,
                PlannedCheckinTime = futureStart,
                PlannedCheckoutTime = futureEnd,
                DepositAmount = 80000m,
                BookingStatus = BookingStatus.Confirmed,
                PaymentDeadline = futureStart.AddMinutes(-15),
                CheckinGraceUntil = futureStart.AddMinutes(30),
                SlotId = fillSlot?.Id
            };
            zc02Bookings.Add(fillBooking);
        }

        await context.AddRangeAsync(zc02Bookings);
        await context.SaveChangesAsync();

        foreach (var b in zc02Bookings)
        {
            zc02Payments.Add(new Payment
            {
                BookingId = b.Id,
                Amount = b.DepositAmount,
                PaymentMethod = "ONLINE",
                PaymentStatus = "PAID",
                OrderCode = DateTime.UtcNow.Ticks + b.Id,
            });
        }
        await context.AddRangeAsync(zc02Payments);
        await context.SaveChangesAsync();
    }

    private static async Task<Vehicle> GetOrCreateVehicleAsync(AppDbContext context, int accountId, int vehicleTypeId, string licensePlate)
    {
        var vehicle = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        if (vehicle == null)
        {
            vehicle = new Vehicle
            {
                AccountId = accountId,
                VehicleTypeId = vehicleTypeId,
                LicensePlate = licensePlate,
                RegisteredDay = DateTime.UtcNow.AddDays(-30),
                VehicleStatus = "ACTIVE"
            };
            await context.AddAsync(vehicle);
            await context.SaveChangesAsync();
        }
        return vehicle;
    }
}
