using GiftHelper.Data.Services.Models;
using GiftHelper.Domain.Entities;
using GiftHelper.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GiftHelper.Data.Services;

public class GiftIdeaService(GiftHelperDbContext dbContext)
{
    public async Task<List<GiftIdea>> GetAllAsync(
        GiftIdeaFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.GiftIdeas
            .AsNoTracking()
            .Include(x => x.Recipient)
            .Include(x => x.Occasion)
            .AsQueryable();

        if (filter is not null)
        {
            if (filter.RecipientId.HasValue)
            {
                query = query.Where(x => x.RecipientId == filter.RecipientId.Value);
            }

            if (filter.OccasionId.HasValue)
            {
                query = query.Where(x => x.OccasionId == filter.OccasionId.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(x => x.Priority == filter.Priority.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim();
                query = query.Where(x =>
                    x.Title.Contains(search) ||
                    (x.Description != null && x.Description.Contains(search)) ||
                    (x.Store != null && x.Store.Contains(search)) ||
                    (x.Category != null && x.Category.Contains(search)));
            }
        }

        return await query
            .OrderByDescending(x => x.UpdatedUtc)
            .ThenBy(x => x.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<GiftIdea?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.GiftIdeas
            .AsNoTracking()
            .Include(x => x.Recipient)
            .Include(x => x.Occasion)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsBySeedIdAsync(string seedId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(seedId))
        {
            return false;
        }

        var normalizedSeedId = seedId.Trim();

        return await dbContext.GiftIdeas
            .AsNoTracking()
            .AnyAsync(x => x.SeedId == normalizedSeedId, cancellationToken);
    }

    public async Task<GiftIdea> CreateAsync(GiftIdea giftIdea, CancellationToken cancellationToken = default)
    {
        dbContext.GiftIdeas.Add(giftIdea);
        await dbContext.SaveChangesAsync(cancellationToken);
        return giftIdea;
    }

    public async Task<bool> UpdateAsync(GiftIdea giftIdea, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.GiftIdeas
            .FirstOrDefaultAsync(x => x.Id == giftIdea.Id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.RecipientId = giftIdea.RecipientId;
        existing.OccasionId = giftIdea.OccasionId;
        existing.Title = giftIdea.Title;
        existing.Description = giftIdea.Description;
        existing.Url = giftIdea.Url;
        existing.Store = giftIdea.Store;
        existing.PriceEstimate = giftIdea.PriceEstimate;
        existing.PricePaid = giftIdea.PricePaid;
        existing.Status = giftIdea.Status;
        existing.Priority = giftIdea.Priority;
        existing.Category = giftIdea.Category;
        existing.EstimatedMinPrice = giftIdea.EstimatedMinPrice ?? existing.EstimatedMinPrice;
        existing.EstimatedMaxPrice = giftIdea.EstimatedMaxPrice ?? existing.EstimatedMaxPrice;
        existing.Tags = giftIdea.Tags ?? existing.Tags;
        existing.SeedId = giftIdea.SeedId ?? existing.SeedId;
        existing.IsSurprise = giftIdea.IsSurprise;
        existing.PurchasedDateUtc = giftIdea.PurchasedDateUtc;
        existing.Notes = giftIdea.Notes;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.GiftIdeas
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        dbContext.GiftIdeas.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<GiftStatusCount>> GetStatusSummaryAsync(CancellationToken cancellationToken = default)
    {
        var grouped = await dbContext.GiftIdeas
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<GiftStatus>()
            .Select(status =>
            {
                var match = grouped.FirstOrDefault(x => x.Status == status);
                var count = match?.Count ?? 0;
                return new GiftStatusCount(status, count);
            })
            .ToList();
    }
}
