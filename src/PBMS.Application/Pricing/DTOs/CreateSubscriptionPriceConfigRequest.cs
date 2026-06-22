namespace PBMS.Application.Pricing.DTOs;

public class CreateSubscriptionPriceConfigRequest
{
    public int VehicleTypeId { get; set; }
    public decimal Price { get; set; }
}
