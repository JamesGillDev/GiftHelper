using GiftHelper.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GiftHelper.Data.Services;

public class RecipientService(GiftHelperDbContext dbContext)
{
    public async Task<List<Recipient>> GetAllAsync(string? searchName = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Recipients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var searchTerm = searchName.Trim();
            query = query.Where(x => x.Name.Contains(searchTerm));
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Recipient>> SearchByNameAsync(string searchName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchName))
        {
            return [];
        }

        var searchTerm = searchName.Trim();
        return await dbContext.Recipients
            .AsNoTracking()
            .Where(x => x.Name.Contains(searchTerm))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recipient?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Recipients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Recipient> CreateAsync(Recipient recipient, CancellationToken cancellationToken = default)
    {
        dbContext.Recipients.Add(recipient);
        await dbContext.SaveChangesAsync(cancellationToken);
        return recipient;
    }

    public async Task<bool> UpdateAsync(Recipient recipient, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Recipients
            .FirstOrDefaultAsync(x => x.Id == recipient.Id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.Name = recipient.Name;
        existing.Relationship = recipient.Relationship;
        existing.Birthday = recipient.Birthday;
        existing.Notes = recipient.Notes;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Recipients
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        dbContext.Recipients.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
