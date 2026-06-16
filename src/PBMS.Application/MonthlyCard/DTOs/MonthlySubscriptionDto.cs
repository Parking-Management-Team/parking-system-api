using System;

namespace PBMS.Application.MonthlyCard.DTOs;

public class MonthlySubscriptionDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int VehicleId { get; set; }
    public int? AssignedCardId { get; set; }
    public string? CardCode { get; set; }
    public int? AssignedSlotId { get; set; }
    public string? SlotCode { get; set; }
    public int BuildingId { get; set; }
    public decimal MonthlyPrice { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public string MonthlySubscriptionStatus { get; set; } = "PENDING";
    public DateTime CreatedAt { get; set; }
}
