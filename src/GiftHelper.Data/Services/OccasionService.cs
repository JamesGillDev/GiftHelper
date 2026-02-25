using GiftHelper.Data.Services.Models;
using GiftHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftHelper.Data.Services;

public class OccasionService(GiftHelperDbContext dbContext)
{
    public async Task<List<Occasion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Occasions
            .AsNoTracking()
            .Include(x => x.Recipient)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Occasion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Occasions
            .AsNoTracking()
            .Include(x => x.Recipient)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Occasion> CreateAsync(Occasion occasion, CancellationToken cancellationToken = default)
    {
        dbContext.Occasions.Add(occasion);
        await dbContext.SaveChangesAsync(cancellationToken);
        return occasion;
    }

    public async Task<bool> UpdateAsync(Occasion occasion, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Occasions
            .FirstOrDefaultAsync(x => x.Id == occasion.Id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.Name = occasion.Name;
        existing.Date = occasion.Date;
        existing.IsRecurringYearly = occasion.IsRecurringYearly;
        existing.RecipientId = occasion.RecipientId;
        existing.Notes = occasion.Notes;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Occasions
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        dbContext.Occasions.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<UpcomingOccasionItem>> GetUpcomingOccasionsAsync(
        int daysAhead,
        int? recipientId = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var safeDaysAhead = Math.Max(daysAhead, 0);

        var query = dbContext.Occasions
            .AsNoTracking()
            .Include(x => x.Recipient)
            .AsQueryable();

        if (recipientId.HasValue)
        {
            query = query.Where(x => x.RecipientId == recipientId.Value);
        }

        var occasions = await query.ToListAsync(cancellationToken);

        return occasions
            .Select(occasion =>
            {
                var projectedDate = ProjectDate(occasion, today);
                var daysUntil = projectedDate.DayNumber - today.DayNumber;

                return new UpcomingOccasionItem
                {
                    OccasionId = occasion.Id,
                    OccasionName = occasion.Name,
                    RecipientId = occasion.RecipientId,
                    RecipientName = occasion.Recipient?.Name,
                    IsRecurringYearly = occasion.IsRecurringYearly,
                    ProjectedDate = projectedDate,
                    DaysUntil = daysUntil
                };
            })
            .Where(item => item.DaysUntil >= 0 && item.DaysUntil <= safeDaysAhead)
            .OrderBy(item => item.ProjectedDate)
            .ToList();
    }

    public DateOnly ProjectDate(Occasion occasion, DateOnly fromDate)
    {
        if (!occasion.IsRecurringYearly)
        {
            return occasion.Date;
        }

        var projectedDate = CreateSafeDate(fromDate.Year, occasion.Date.Month, occasion.Date.Day);

        if (projectedDate < fromDate)
        {
            projectedDate = CreateSafeDate(fromDate.Year + 1, occasion.Date.Month, occasion.Date.Day);
        }

        return projectedDate;
    }

    private static DateOnly CreateSafeDate(int year, int month, int day)
    {
        var safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, safeDay);
    }
}
