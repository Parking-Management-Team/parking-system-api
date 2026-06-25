namespace PBMS.Application.MonthlyCard.DTOs;

/// <summary>
/// Request to update a monthly subscription.
/// </summary>
public class UpdateSubscriptionRequest
{
    /// <summary>
    /// New MONTHLY card ID to assign/replace.
    /// </summary>
    public int? AssignedCardId { get; set; }
}