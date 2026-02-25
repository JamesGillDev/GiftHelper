using GiftHelper.Domain.Enums;

namespace GiftHelper.Data.Services.Models;

public class GiftIdeaFilter
{
    public int? RecipientId { get; set; }

    public int? OccasionId { get; set; }

    public GiftStatus? Status { get; set; }

    public GiftPriority? Priority { get; set; }

    public string? SearchText { get; set; }
}
