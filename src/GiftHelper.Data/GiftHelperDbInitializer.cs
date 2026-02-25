using GiftHelper.Domain.Entities;
using GiftHelper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GiftHelper.Data;

public static class GiftHelperDbInitializer
{
    public static async Task InitializeAsync(GiftHelperDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        var hasData =
            await dbContext.Recipients.AnyAsync(cancellationToken) ||
            await dbContext.Occasions.AnyAsync(cancellationToken) ||
            await dbContext.GiftIdeas.AnyAsync(cancellationToken);

        if (hasData)
        {
            return;
        }

        var cassie = new Recipient
        {
            Name = "Cassie",
            Relationship = "Sister",
            Birthday = new DateOnly(1994, 8, 11),
            Notes = "Loves home decor and practical gifts."
        };

        var jordan = new Recipient
        {
            Name = "Jordan",
            Relationship = "Friend",
            Birthday = new DateOnly(1992, 3, 2),
            Notes = "Enjoys coffee, fitness, and board games."
        };

        dbContext.Recipients.AddRange(cassie, jordan);
        await dbContext.SaveChangesAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var christmas = new Occasion
        {
            Name = "Christmas",
            Date = new DateOnly(today.Year, 12, 25),
            IsRecurringYearly = true,
            Notes = "Family gathering and gift exchange."
        };

        var cassieBirthday = new Occasion
        {
            Name = "Cassie's Birthday",
            Date = new DateOnly(today.Year, 8, 11),
            IsRecurringYearly = true,
            RecipientId = cassie.Id
        };

        var anniversary = new Occasion
        {
            Name = "Anniversary",
            Date = new DateOnly(today.Year, 6, 15),
            IsRecurringYearly = true,
            Notes = "Dinner and small keepsake."
        };

        dbContext.Occasions.AddRange(christmas, cassieBirthday, anniversary);
        await dbContext.SaveChangesAsync(cancellationToken);

        var giftIdeas = new List<GiftIdea>
        {
            new()
            {
                RecipientId = cassie.Id,
                OccasionId = cassieBirthday.Id,
                Title = "Spa Gift Card",
                Description = "Weekend spa package at her favorite place.",
                Store = "Willow Spa",
                PriceEstimate = 90m,
                Status = GiftStatus.Researching,
                Priority = GiftPriority.High,
                Category = "Experiences",
                IsSurprise = true
            },
            new()
            {
                RecipientId = jordan.Id,
                OccasionId = christmas.Id,
                Title = "Coffee Subscription Box",
                Description = "Monthly specialty beans subscription.",
                Url = "https://example.com/coffee-subscription",
                Store = "Roasters Co.",
                PriceEstimate = 65m,
                Status = GiftStatus.Idea,
                Priority = GiftPriority.Medium,
                Category = "Food & Drink",
                IsSurprise = true
            },
            new()
            {
                RecipientId = cassie.Id,
                Title = "Noise-Cancelling Headphones",
                Store = "Tech Hub",
                PriceEstimate = 230m,
                PricePaid = 199.99m,
                Status = GiftStatus.Purchased,
                Priority = GiftPriority.High,
                Category = "Electronics",
                IsSurprise = false,
                PurchasedDateUtc = DateTime.UtcNow.AddDays(-12)
            }
        };

        dbContext.GiftIdeas.AddRange(giftIdeas);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
