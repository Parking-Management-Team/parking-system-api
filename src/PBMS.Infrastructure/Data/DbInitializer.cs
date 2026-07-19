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

            // Floor 2 has Car Zone (ZC02 - 50 slots)
            var zoneCarF2 = new Zone 
            { 
                FloorId = floor2.Id, 
                Code = "ZC02", 
                Name = "Car Zone F2", 
                Capacity = 50, 
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

            // Seed Slots for Floor 2 - Car Zone (50 slots)
            for (int i = 1; i <= 50; i++)
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
    catch (Exception)
    {
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
        var staff = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "staff")
            ?? throw new InvalidOperationException("Required staff account was not found.");
        var testDriver = await context.Set<Account>().FirstOrDefaultAsync(a => a.Username == "driver")
            ?? throw new InvalidOperationException("Required default driver account was not found.");
        var testVehicle = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == "51G-12345")
            ?? throw new InvalidOperationException("Required default vehicle '51G-12345' was not found.");
        var card1 = await context.Set<Card>().FirstOrDefaultAsync(c => c.CardCode == "CARD001")
            ?? throw new InvalidOperationException("Required card 'CARD001' was not found.");
        var card2 = await context.Set<Card>().FirstOrDefaultAsync(c => c.CardCode == "CARD002")
            ?? throw new InvalidOperationException("Required card 'CARD002' was not found.");
        var building = await context.Set<Building>().FirstOrDefaultAsync(b => b.Code == "BLD01")
            ?? throw new InvalidOperationException("Required building 'BLD01' was not found.");
        var zone = await context.Set<Zone>().FirstOrDefaultAsync(z => z.Code == "ZC01")
            ?? throw new InvalidOperationException("Required zone 'ZC01' was not found.");
        var slot1 = await context.Set<ParkingSlot>().FirstOrDefaultAsync(s => s.Code == "ZC01-01")
            ?? throw new InvalidOperationException("Required slot 'ZC01-01' was not found.");

        // Seeding driver2
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

        // Seeding vehicle2
        var vehicle2 = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == "51G-67890");
        if (vehicle2 == null)
        {
            vehicle2 = new Vehicle
            {
                AccountId = driver2.Id,
                VehicleTypeId = carType.Id,
                LicensePlate = "51G-67890",
                RegisteredDay = DateTime.UtcNow.AddHours(7),
                VehicleStatus = "ACTIVE"
            };
            await context.AddAsync(vehicle2);
            await context.SaveChangesAsync();
        }

        // Seeding driver3
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

        // Seeding vehicle3
        var vehicle3 = await context.Set<Vehicle>().FirstOrDefaultAsync(v => v.LicensePlate == "51G-88888");
        if (vehicle3 == null)
        {
            vehicle3 = new Vehicle
            {
                AccountId = driver3.Id,
                VehicleTypeId = carType.Id,
                LicensePlate = "51G-88888",
                RegisteredDay = DateTime.UtcNow.AddHours(7),
                VehicleStatus = "ACTIVE"
            };
            await context.AddAsync(vehicle3);
            await context.SaveChangesAsync();
        }

        // ----------------------------------------------------
        // A. Past Data (Completed sessions and bookings)
        // ----------------------------------------------------
        var pastBooking = new Booking
        {
            AccountId = testDriver.Id,
            VehicleId = testVehicle.Id,
            VehicleTypeId = carType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddDays(-3).AddHours(-4),
            PlannedCheckoutTime = DateTime.UtcNow.AddDays(-3).AddHours(2),
            DepositAmount = CalculateEstimatedFee(false, DateTime.UtcNow.AddDays(-3).AddHours(-4), DateTime.UtcNow.AddDays(-3).AddHours(2)),
            BookingStatus = BookingStatus.CheckedIn,
            PaymentDeadline = DateTime.UtcNow.AddDays(-3).AddHours(-4).AddMinutes(15),
            CheckinGraceUntil = DateTime.UtcNow.AddDays(-3).AddHours(-4).AddMinutes(30),
            SlotId = slot1.Id
        };
        await context.AddAsync(pastBooking);
        await context.SaveChangesAsync();

        var pastSession = new ParkingSession
        {
            VehicleId = testVehicle.Id,
            BuildingId = building.Id,
            CardId = card1.Id,
            ZoneId = zone.Id,
            SlotId = slot1.Id,
            BookingId = pastBooking.Id,
            CheckInTime = DateTime.UtcNow.AddDays(-3).AddHours(-3).AddMinutes(45),
            CheckOutTime = DateTime.UtcNow.AddDays(-3).AddHours(1).AddMinutes(30),
            LicensePlateIn = testVehicle.LicensePlate,
            LicensePlateOut = testVehicle.LicensePlate,
            SessionStatus = "COMPLETED",
            InStaffId = staff.Id,
            OutStaffId = staff.Id
        };
        await context.AddAsync(pastSession);
        await context.SaveChangesAsync();

        var pastPayment = new Payment
        {
            SessionId = pastSession.Id,
            Amount = 100000m,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.AddDays(-3).Ticks,
            PaymentTime = DateTime.UtcNow.AddDays(-3).AddHours(-3).AddMinutes(40)
        };
        await context.AddAsync(pastPayment);
        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // B. Present Data (Active sessions / Overdue booking)
        // ----------------------------------------------------
        var activeBookingD2 = new Booking
        {
            AccountId = driver2.Id,
            VehicleId = vehicle2.Id,
            VehicleTypeId = carType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddHours(-3),
            PlannedCheckoutTime = DateTime.UtcNow.AddMinutes(-30), // Overdue Checkout!
            DepositAmount = CalculateEstimatedFee(false, DateTime.UtcNow.AddHours(-3), DateTime.UtcNow.AddMinutes(-30)),
            BookingStatus = BookingStatus.CheckedIn,
            PaymentDeadline = DateTime.UtcNow.AddHours(-3).AddMinutes(15),
            CheckinGraceUntil = DateTime.UtcNow.AddHours(-3).AddMinutes(30),
            SlotId = slot1.Id
        };
        await context.AddAsync(activeBookingD2);
        await context.SaveChangesAsync();

        var activeSessionD2 = new ParkingSession
        {
            VehicleId = vehicle2.Id,
            BuildingId = building.Id,
            CardId = card2.Id,
            ZoneId = zone.Id,
            SlotId = slot1.Id,
            BookingId = activeBookingD2.Id,
            CheckInTime = DateTime.UtcNow.AddHours(-2).AddMinutes(50),
            CheckOutTime = null,
            LicensePlateIn = vehicle2.LicensePlate,
            SessionStatus = "ACTIVE",
            InStaffId = staff.Id
        };
        await context.AddAsync(activeSessionD2);
        await context.SaveChangesAsync();

        card2.CardStatus = CardStatus.Active.ToString();
        context.Update(card2);

        slot1.Status = SlotStatus.Occupied;
        context.Update(slot1);
        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // C. Future Data (Confirmed bookings in the future)
        // ----------------------------------------------------
        var futureBookingD3Deposit = CalculateEstimatedFee(false, DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.AddHours(4).AddMinutes(15));
        var futureBookingD3 = new Booking
        {
            AccountId = driver3.Id,
            VehicleId = vehicle3.Id,
            VehicleTypeId = carType.Id,
            BuildingId = building.Id,
            PlannedCheckinTime = DateTime.UtcNow.AddMinutes(15),
            PlannedCheckoutTime = DateTime.UtcNow.AddHours(4).AddMinutes(15),
            DepositAmount = futureBookingD3Deposit,
            BookingStatus = BookingStatus.Confirmed,
            PaymentDeadline = DateTime.UtcNow.AddMinutes(15).AddMinutes(15),
            CheckinGraceUntil = DateTime.UtcNow.AddMinutes(15).AddMinutes(30),
            SlotId = slot1.Id
        };
        await context.AddAsync(futureBookingD3);
        await context.SaveChangesAsync();

        var paymentD3 = new Payment
        {
            BookingId = futureBookingD3.Id,
            Amount = futureBookingD3Deposit,
            PaymentMethod = "ONLINE",
            PaymentStatus = "PAID",
            OrderCode = DateTime.UtcNow.Ticks,
            PaymentTime = DateTime.UtcNow.AddMinutes(-5)
        };
        await context.AddAsync(paymentD3);
        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // D. Historical Data for the last 30 days
        // ----------------------------------------------------
        var random = new Random();
        var allVehicles = await context.Set<Vehicle>().ToListAsync();
        var allCards = await context.Set<Card>().ToListAsync();
        var allSlots = await context.Set<ParkingSlot>().ToListAsync();
        var allZones = await context.Set<Zone>().ToListAsync();
        var incidentTypes = await context.Set<IncidentType>().ToListAsync();
        var penaltyConfigs = await context.Set<PenaltyConfig>().ToListAsync();

        var motorcycleType = await context.Set<VehicleType>().FirstOrDefaultAsync(v => v.VehicleTypeCode == "MOTOR");

        // Create extra vehicles to make historical data richer
        if (allVehicles.Count < 6)
        {
            var extraVehicles = new List<Vehicle>
            {
                new Vehicle { AccountId = testDriver.Id, VehicleTypeId = motorcycleType!.Id, LicensePlate = "59T1-11111", RegisteredDay = DateTime.UtcNow.AddDays(-40), VehicleStatus = "ACTIVE" },
                new Vehicle { AccountId = testDriver.Id, VehicleTypeId = carType.Id, LicensePlate = "51H-22222", RegisteredDay = DateTime.UtcNow.AddDays(-40), VehicleStatus = "ACTIVE" },
                new Vehicle { AccountId = driver2.Id, VehicleTypeId = motorcycleType!.Id, LicensePlate = "59T1-33333", RegisteredDay = DateTime.UtcNow.AddDays(-40), VehicleStatus = "ACTIVE" },
                new Vehicle { AccountId = driver3.Id, VehicleTypeId = motorcycleType!.Id, LicensePlate = "59T1-44444", RegisteredDay = DateTime.UtcNow.AddDays(-40), VehicleStatus = "ACTIVE" },
                new Vehicle { AccountId = driver3.Id, VehicleTypeId = carType.Id, LicensePlate = "51H-55555", RegisteredDay = DateTime.UtcNow.AddDays(-40), VehicleStatus = "ACTIVE" }
            };
            await context.AddRangeAsync(extraVehicles);
            await context.SaveChangesAsync();
            allVehicles.AddRange(extraVehicles);
        }

        var today = DateTime.UtcNow.Date;
        var histBookings = new List<Booking>();
        var histSessions = new List<ParkingSession>();
        var histPayments = new List<Payment>();
        var histIncidents = new List<Incident>();

        for (int dayOffset = 30; dayOffset >= 1; dayOffset--)
        {
            var targetDate = today.AddDays(-dayOffset);
            int sessionCount = random.Next(3, 8); // 3 to 7 sessions per day
            
            for (int s = 0; s < sessionCount; s++)
            {
                var vehicle = allVehicles[random.Next(allVehicles.Count)];
                var isMotor = vehicle.VehicleTypeId == motorcycleType!.Id;
                
                var card = allCards[random.Next(allCards.Count)];
                var availableSlots = allSlots.Where(sl => sl.VehicleTypeId == vehicle.VehicleTypeId).ToList();
                if (!availableSlots.Any()) continue;
                
                var slot = availableSlots[random.Next(availableSlots.Count)];
                var currentZone = allZones.FirstOrDefault(z => z.Id == slot.ZoneId);
                
                int checkinHour = random.Next(7, 19);
                int checkinMinute = random.Next(0, 60);
                var checkInTime = targetDate.AddHours(checkinHour).AddMinutes(checkinMinute);
                
                int durationMinutes = random.Next(60, 480); // 1 to 8 hours
                var checkOutTime = checkInTime.AddMinutes(durationMinutes);
                
                Booking? booking = null;
                var isBooking = random.Next(0, 100) < 30;
                
                if (isBooking)
                {
                    var plannedCheckin = checkInTime.AddMinutes(random.Next(-30, 15));
                    var plannedCheckout = checkOutTime.AddMinutes(random.Next(-15, 30));
                    booking = new Booking
                    {
                        AccountId = vehicle.AccountId ?? testDriver.Id,
                        VehicleId = vehicle.Id,
                        VehicleTypeId = vehicle.VehicleTypeId,
                        BuildingId = building.Id,
                        PlannedCheckinTime = plannedCheckin,
                        PlannedCheckoutTime = plannedCheckout,
                        DepositAmount = CalculateEstimatedFee(isMotor, plannedCheckin, plannedCheckout),
                        BookingStatus = BookingStatus.CheckedIn,
                        PaymentDeadline = plannedCheckin.AddMinutes(-30),
                        CheckinGraceUntil = plannedCheckin.AddMinutes(30),
                        SlotId = isMotor ? null : slot.Id
                    };
                    histBookings.Add(booking);
                }
                
                var session = new ParkingSession
                {
                    VehicleId = vehicle.Id,
                    BuildingId = building.Id,
                    CardId = card.Id,
                    ZoneId = currentZone?.Id,
                    SlotId = slot.Id,
                    Booking = booking,
                    CheckInTime = checkInTime,
                    CheckOutTime = checkOutTime,
                    LicensePlateIn = vehicle.LicensePlate,
                    LicensePlateOut = vehicle.LicensePlate,
                    SessionStatus = SessionStatus.Completed,
                    InStaffId = staff.Id,
                    OutStaffId = staff.Id
                };
                histSessions.Add(session);
                
                // Calculate amount
                decimal calculatedAmount = CalculateEstimatedFee(isMotor, checkInTime, checkOutTime);
                
                var payment = new Payment
                {
                    Session = session,
                    Amount = calculatedAmount,
                    PaymentMethod = random.Next(0, 100) < 50 ? "CASH" : "ONLINE",
                    PaymentStatus = "PAID",
                    OrderCode = checkOutTime.Ticks,
                    PaymentTime = checkOutTime
                };
                histPayments.Add(payment);
                
                // 3% Incident rate
                if (random.Next(0, 100) < 3 && incidentTypes.Any())
                {
                    var it = incidentTypes[random.Next(incidentTypes.Count)];
                    var pc = penaltyConfigs.FirstOrDefault(c => c.IncidentTypeId == it.Id);
                    
                    var incident = new Incident
                    {
                        Session = session,
                        IncidentTypeId = it.Id,
                        Description = $"Demo incident: {it.IncidentName}",
                        PenaltyFee = pc?.PenaltyFee ?? 50000m,
                        PenaltyConfigId = pc?.Id,
                        Status = IncidentStatus.Resolved,
                        ResolvedAt = checkOutTime
                    };
                    histIncidents.Add(incident);
                    payment.Amount += incident.PenaltyFee ?? 0;
                }
            }
        }

        if (histBookings.Any()) await context.AddRangeAsync(histBookings);
        await context.AddRangeAsync(histSessions);
        await context.AddRangeAsync(histPayments);
        if (histIncidents.Any()) await context.AddRangeAsync(histIncidents);
        
        await context.SaveChangesAsync();

        // ----------------------------------------------------
        // E. Blacklisted Vehicle Seeding (Merged from develop)
        // ----------------------------------------------------
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
    }

    private static decimal CalculateEstimatedFee(bool isMotor, DateTime start, DateTime end)
    {
        double durationMinutes = (end - start).TotalMinutes;
        if (durationMinutes <= 0) return 0;
        
        decimal basePrice = isMotor ? 5000m : 20000m;
        decimal calculatedAmount = basePrice;
        if (durationMinutes > 60)
        {
            var extraMinutes = durationMinutes - 60;
            var incrementBlock = 15;
            var incrementPrice = isMotor ? 2000m : 5000m;
            var blocks = (int)Math.Ceiling(extraMinutes / incrementBlock);
            calculatedAmount += blocks * incrementPrice;
        }
        var maxCap = isMotor ? 50000m : 150000m;
        if (calculatedAmount > maxCap)
        {
            calculatedAmount = maxCap;
        }
        return calculatedAmount;
    }
}
