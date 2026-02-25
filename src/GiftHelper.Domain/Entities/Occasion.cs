using System.ComponentModel.DataAnnotations;

namespace GiftHelper.Domain.Entities;

public class Occasion
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }

    public bool IsRecurringYearly { get; set; } = true;

    public int? RecipientId { get; set; }

    public Recipient? Recipient { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public ICollection<GiftIdea> GiftIdeas { get; set; } = new List<GiftIdea>();
}
