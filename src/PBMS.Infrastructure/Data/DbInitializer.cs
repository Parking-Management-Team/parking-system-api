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
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod",
                            ExecutionOrder = 1,
                            IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing",
                            ExecutionOrder = 2,
                            IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig
                            {
                                BaseDurationMinutes = 60,
                                BasePriceAmount = 5000m,
                                CurrencyCode = "VND"
                            }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing",
                            ExecutionOrder = 3,
                            IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig
                            {
                                IncrementIntervalMinutes = 15,
                                IncrementPriceAmount = 2000m,
                                ThresholdPercentage = 50,
                                CurrencyCode = "VND"
                            }
                        },
                        new PricingRule
                        {
                            RuleType = "DailyCap",
                            ExecutionOrder = 4,
                            IsActive = true,
                            DailyCapRuleConfig = new DailyCapRuleConfig
                            {
                                MaximumDailyAmount = 50000m,
                                CurrencyCode = "VND"
                            }
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
                    },
                    PricingRules = new List<PricingRule>
                    {
                        new PricingRule
                        {
                            RuleType = "GracePeriod",
                            ExecutionOrder = 1,
                            IsActive = true,
                            GracePeriodRuleConfig = new GracePeriodRuleConfig { GracePeriodMinutes = 15 }
                        },
                        new PricingRule
                        {
                            RuleType = "BasePricing",
                            ExecutionOrder = 2,
                            IsActive = true,
                            BasePricingRuleConfig = new BasePricingRuleConfig
                            {
                                BaseDurationMinutes = 60,
                                BasePriceAmount = 20000m,
                                CurrencyCode = "VND"
                            }
                        },
                        new PricingRule
                        {
                            RuleType = "IncrementPricing",
                            ExecutionOrder = 3,
                            IsActive = true,
                            IncrementPricingRuleConfig = new IncrementPricingRuleConfig
                            {
                                IncrementIntervalMinutes = 15,
                                IncrementPriceAmount = 5000m,
                                ThresholdPercentage = 50,
                                CurrencyCode = "VND"
                            }
                        },
                        new PricingRule
                        {
                            RuleType = "DailyCap",
                            ExecutionOrder = 4,
                            IsActive = true,
                            DailyCapRuleConfig = new DailyCapRuleConfig
                            {
                                MaximumDailyAmount = 150000m,
                                CurrencyCode = "VND"
                            }
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
            var motorcycleConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = motorcycleType!.Id,
                Price = 150000, // 150k/tháng
                DurationDays = 30,
                EffectiveFrom = DateTime.UtcNow,
                IsActive = true
            };

            var carConfig = new SubscriptionPriceConfig
            {
                VehicleTypeId = carType!.Id,
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
    }
}
