using System.ComponentModel.DataAnnotations;

namespace GiftHelper.Domain.Entities;

public class GiftSearchProfile
{
    [Required]
    [StringLength(100)]
    public string Relationship { get; set; } = "friend";

    [Required]
    [StringLength(100)]
    public string Occasion { get; set; } = "birthday";

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Budget min must be 0 or greater.")]
    public decimal? BudgetMin { get; set; }

    [Range(typeof(decimal), "0", "1000000", ErrorMessage = "Budget max must be 0 or greater.")]
    public decimal? BudgetMax { get; set; }

    public List<string> InterestTags { get; set; } = [];

    [StringLength(500)]
    public string? InterestFreeText { get; set; }

    [StringLength(2000)]
    public string? Constraints { get; set; }

    [Required]
    [StringLength(50)]
    public string Style { get; set; } = "practical";

    [Required]
    [StringLength(50)]
    public string ShippingTimeline { get; set; } = "normal";

    [StringLength(50)]
    public string? AgeRange { get; set; }

    public bool AlreadyHasEverything { get; set; }
}
