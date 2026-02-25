namespace GiftHelper.Data.Services.Models;

public class UpcomingOccasionItem
{
    public int OccasionId { get; init; }

    public string OccasionName { get; init; } = string.Empty;

    public int? RecipientId { get; init; }

    public string? RecipientName { get; init; }

    public bool IsRecurringYearly { get; init; }

    public DateOnly ProjectedDate { get; init; }

    public int DaysUntil { get; init; }
}
