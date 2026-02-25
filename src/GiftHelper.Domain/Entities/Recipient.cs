using System.ComponentModel.DataAnnotations;

namespace GiftHelper.Domain.Entities;

public class Recipient
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Relationship { get; set; }

    public DateOnly? Birthday { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public ICollection<Occasion> Occasions { get; set; } = new List<Occasion>();

    public ICollection<GiftIdea> GiftIdeas { get; set; } = new List<GiftIdea>();
}
