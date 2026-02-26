using System.ComponentModel.DataAnnotations;

namespace GiftHelper.Domain.Entities;

public class GiftSuggestion
{
    [Required]
    [StringLength(100)]
    public string SeedId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    public decimal MinPrice { get; set; }

    public decimal MaxPrice { get; set; }

    [Required]
    [StringLength(500)]
    public string WhyItFits { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    public string SearchUrl { get; set; } = string.Empty;

    [StringLength(500)]
    public string? MatchedTags { get; set; }

    public double Score { get; set; }
}
