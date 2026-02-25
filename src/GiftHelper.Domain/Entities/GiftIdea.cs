using System.ComponentModel.DataAnnotations;
using GiftHelper.Domain.Enums;

namespace GiftHelper.Domain.Entities;

public class GiftIdea
{
    public int Id { get; set; }

    [Required]
    public int RecipientId { get; set; }

    public Recipient Recipient { get; set; } = null!;

    public int? OccasionId { get; set; }

    public Occasion? Occasion { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Url]
    [StringLength(500)]
    public string? Url { get; set; }

    [StringLength(200)]
    public string? Store { get; set; }

    public decimal? PriceEstimate { get; set; }

    public decimal? PricePaid { get; set; }

    public GiftStatus Status { get; set; } = GiftStatus.Idea;

    public GiftPriority Priority { get; set; } = GiftPriority.Medium;

    [StringLength(100)]
    public string? Category { get; set; }

    public bool IsSurprise { get; set; } = true;

    public DateTime? PurchasedDateUtc { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }
}
