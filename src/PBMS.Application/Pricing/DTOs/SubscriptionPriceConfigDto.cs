using System;

namespace PBMS.Application.Pricing.DTOs;

public class SubscriptionPriceConfigDto
{
    public int Id { get; set; }
    public int VehicleTypeId { get; set; }
    public string VehicleTypeName { get; set; } = null!;
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}
