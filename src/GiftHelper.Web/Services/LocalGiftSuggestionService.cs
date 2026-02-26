using System.Text.Json;
using GiftHelper.Domain.Entities;

namespace GiftHelper.Web.Services;

public class LocalGiftSuggestionService(IWebHostEnvironment environment, ILogger<LocalGiftSuggestionService> logger)
{
    private static readonly JsonSerializerOptions SeedJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _catalogPath = Path.Combine(
        environment.ContentRootPath,
        "Data",
        "Seed",
        "gift_ideas.json");

    private readonly SemaphoreSlim _catalogLock = new(1, 1);
    private List<SeedGiftIdea>? _catalogCache;

    public async Task<IReadOnlyList<GiftSuggestion>> GetSuggestionsAsync(
        GiftSearchProfile profile,
        int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var normalizedProfile = NormalizeProfile(profile);
        var userConstraintRejectTags = ExtractUserConstraintRejectTags(normalizedProfile);
        var catalog = await LoadCatalogAsync(cancellationToken);

        if (catalog.Count == 0)
        {
            return [];
        }

        var ranked = catalog
            .Select(item => ScoreItem(item, normalizedProfile, userConstraintRejectTags))
            .Where(result => !result.IsRejected)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Item.Title)
            .Take(Math.Clamp(maxResults, 1, 20))
            .Select(BuildSuggestion)
            .ToList();

        return ranked;
    }

    private async Task<List<SeedGiftIdea>> LoadCatalogAsync(CancellationToken cancellationToken)
    {
        if (_catalogCache is not null)
        {
            return _catalogCache;
        }

        await _catalogLock.WaitAsync(cancellationToken);

        try
        {
            if (_catalogCache is not null)
            {
                return _catalogCache;
            }

            if (!File.Exists(_catalogPath))
            {
                logger.LogWarning("Gift seed file missing at {CatalogPath}", _catalogPath);
                _catalogCache = [];
                return _catalogCache;
            }

            var seedJson = await File.ReadAllTextAsync(_catalogPath, cancellationToken);
            var parsed = JsonSerializer.Deserialize<List<SeedGiftIdea>>(seedJson, SeedJsonOptions) ?? [];

            foreach (var item in parsed)
            {
                NormalizeSeedItem(item);
            }

            _catalogCache = parsed;
            return _catalogCache;
        }
        finally
        {
            _catalogLock.Release();
        }
    }

    private static ScoreResult ScoreItem(
        SeedGiftIdea item,
        GiftSearchProfile profile,
        HashSet<string> userConstraintRejectTags)
    {
        var itemConstraintTags = item.ConstraintTags.ToHashSet(StringComparer.Ordinal);

        if (userConstraintRejectTags.Overlaps(itemConstraintTags))
        {
            return new ScoreResult(item, -999, true, "Constraints conflict.", []);
        }

        var score = 0;
        var reasons = new List<string>();
        var matchedTags = new List<string>();

        var relationshipScore = ScoreTagMatch(
            item.RelationshipTags,
            profile.Relationship,
            exactPoints: 30,
            partialPoints: 15,
            out var relationshipMatch);

        score += relationshipScore;

        if (!string.IsNullOrWhiteSpace(relationshipMatch))
        {
            reasons.Add($"relationship match ({relationshipMatch})");
            matchedTags.Add(relationshipMatch);
        }

        var occasionScore = ScoreTagMatch(
            item.OccasionTags,
            profile.Occasion,
            exactPoints: 20,
            partialPoints: 10,
            out var occasionMatch);

        score += occasionScore;

        if (!string.IsNullOrWhiteSpace(occasionMatch))
        {
            reasons.Add($"occasion fit ({occasionMatch})");
            matchedTags.Add(occasionMatch);
        }

        var interestMatches = GetInterestMatches(item, profile);
        var interestScore = Math.Min(interestMatches.Count * 8, 40);
        score += interestScore;

        if (interestMatches.Count > 0)
        {
            reasons.Add($"interest overlap ({string.Join(", ", interestMatches)})");
            matchedTags.AddRange(interestMatches);
        }

        if (item.StyleTags.Contains(profile.Style, StringComparer.Ordinal))
        {
            score += 15;
            reasons.Add($"style match ({profile.Style})");
            matchedTags.Add(profile.Style);
        }

        score += ScoreBudget(item, profile, out var budgetReason);

        if (!string.IsNullOrWhiteSpace(budgetReason))
        {
            reasons.Add(budgetReason);
        }

        if (profile.ShippingTimeline == "need-fast" &&
            (item.ConstraintTags.Contains("custom-made", StringComparer.Ordinal) ||
             item.ConstraintTags.Contains("slow-ship", StringComparer.Ordinal)))
        {
            score -= 15;
            reasons.Add("slower fulfillment risk for urgent timeline");
        }

        if (!string.IsNullOrWhiteSpace(profile.AgeRange))
        {
            var ageTag = $"age-{profile.AgeRange}";

            if (item.InterestTags.Contains(ageTag, StringComparer.Ordinal))
            {
                score += 8;
                reasons.Add($"age-range fit ({profile.AgeRange})");
                matchedTags.Add(ageTag);
            }
        }

        if (profile.AlreadyHasEverything &&
            !item.ConstraintTags.Contains("clutter", StringComparer.Ordinal))
        {
            score += 8;
            reasons.Add("low-clutter friendly option");
        }

        var rationale = BuildRationale(item, reasons);
        return new ScoreResult(item, score, false, rationale, matchedTags.Distinct(StringComparer.Ordinal).ToList());
    }

    private static int ScoreTagMatch(
        List<string> itemTags,
        string answer,
        int exactPoints,
        int partialPoints,
        out string matchedTag)
    {
        matchedTag = string.Empty;

        if (string.IsNullOrWhiteSpace(answer) || itemTags.Count == 0)
        {
            return 0;
        }

        if (itemTags.Contains(answer, StringComparer.Ordinal))
        {
            matchedTag = answer;
            return exactPoints;
        }

        if (itemTags.Contains("any", StringComparer.Ordinal))
        {
            matchedTag = "any";
            return partialPoints;
        }

        var answerTokens = SplitTagTokens(answer);

        foreach (var tag in itemTags)
        {
            var tagTokens = SplitTagTokens(tag);

            if (answerTokens.Overlaps(tagTokens))
            {
                matchedTag = tag;
                return partialPoints;
            }
        }

        return 0;
    }

    private static List<string> GetInterestMatches(SeedGiftIdea item, GiftSearchProfile profile)
    {
        var matches = new HashSet<string>(StringComparer.Ordinal);

        var profileTags = profile.InterestTags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .ToList();

        foreach (var tag in profileTags)
        {
            if (item.InterestTags.Contains(tag, StringComparer.Ordinal))
            {
                matches.Add(tag);
            }
        }

        var freeTextTokens = SplitTagTokens(profile.InterestFreeText ?? string.Empty);

        foreach (var token in freeTextTokens)
        {
            var match = item.InterestTags.FirstOrDefault(x => x.Contains(token, StringComparison.Ordinal));

            if (!string.IsNullOrWhiteSpace(match))
            {
                matches.Add(match);
            }
        }

        return matches.ToList();
    }

    private static int ScoreBudget(SeedGiftIdea item, GiftSearchProfile profile, out string reason)
    {
        reason = string.Empty;

        if (!profile.BudgetMin.HasValue && !profile.BudgetMax.HasValue)
        {
            return 0;
        }

        var minBudget = profile.BudgetMin ?? 0m;
        var maxBudget = profile.BudgetMax ?? decimal.MaxValue;

        if (profile.BudgetMin.HasValue && profile.BudgetMax.HasValue && minBudget > maxBudget)
        {
            (minBudget, maxBudget) = (maxBudget, minBudget);
        }

        var inRange = item.MaxPrice >= minBudget && item.MinPrice <= maxBudget;

        if (inRange)
        {
            reason = "budget aligned";
            return 25;
        }

        if (item.MinPrice > maxBudget && maxBudget > 0)
        {
            var overBy = (item.MinPrice - maxBudget) / maxBudget;

            if (overBy <= 0.10m)
            {
                reason = "slightly over budget";
                return 10;
            }
        }

        reason = "outside budget range";
        return -20;
    }

    private static GiftSuggestion BuildSuggestion(ScoreResult result)
    {
        var searchTerms = string.IsNullOrWhiteSpace(result.Item.StoreSearchTerms)
            ? $"{result.Item.Title} {result.Item.Category} gift"
            : result.Item.StoreSearchTerms;

        var encodedSearchTerms = Uri.EscapeDataString(searchTerms);
        var searchUrl = $"https://www.google.com/search?q={encodedSearchTerms}";

        return new GiftSuggestion
        {
            SeedId = result.Item.Id,
            Title = result.Item.Title,
            Category = result.Item.Category,
            MinPrice = result.Item.MinPrice,
            MaxPrice = result.Item.MaxPrice,
            WhyItFits = result.Rationale,
            SearchUrl = searchUrl,
            MatchedTags = result.MatchedTags.Count == 0
                ? null
                : string.Join(", ", result.MatchedTags),
            Score = result.Score
        };
    }

    private static string BuildRationale(SeedGiftIdea item, List<string> reasons)
    {
        var reasonText = reasons.Count == 0
            ? "General match from your profile."
            : $"Matches {string.Join("; ", reasons)}.";

        var hint = string.IsNullOrWhiteSpace(item.Notes)
            ? string.Empty
            : $" {item.Notes.Trim()}";

        var priceRange = item.MinPrice == item.MaxPrice
            ? item.MinPrice.ToString("C0")
            : $"{item.MinPrice:C0}-{item.MaxPrice:C0}";

        return $"{reasonText} Estimated range: {priceRange}.{hint}";
    }

    private static HashSet<string> ExtractUserConstraintRejectTags(GiftSearchProfile profile)
    {
        var tags = new HashSet<string>(StringComparer.Ordinal);
        var constraints = (profile.Constraints ?? string.Empty).ToLowerInvariant();

        if (profile.AlreadyHasEverything)
        {
            tags.Add("clutter");
        }

        if (constraints.Contains("allerg", StringComparison.Ordinal) ||
            constraints.Contains("non-food", StringComparison.Ordinal) ||
            constraints.Contains("no food", StringComparison.Ordinal))
        {
            tags.Add("food");
        }

        if (constraints.Contains("doesn't drink", StringComparison.Ordinal) ||
            constraints.Contains("doesnt drink", StringComparison.Ordinal) ||
            constraints.Contains("no alcohol", StringComparison.Ordinal) ||
            constraints.Contains("sober", StringComparison.Ordinal))
        {
            tags.Add("alcohol");
        }

        if (constraints.Contains("clutter", StringComparison.Ordinal) ||
            constraints.Contains("minimalist", StringComparison.Ordinal))
        {
            tags.Add("clutter");
        }

        if (constraints.Contains("pet", StringComparison.Ordinal) &&
            constraints.Contains("allerg", StringComparison.Ordinal))
        {
            tags.Add("pet-unfriendly");
        }

        return tags;
    }

    private static HashSet<string> SplitTagTokens(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var separators = new[] { ' ', '-', '/', '_', ',', ';', '.', ':', '|', '&' };

        return value
            .Trim()
            .ToLowerInvariant()
            .Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length > 1)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static GiftSearchProfile NormalizeProfile(GiftSearchProfile profile)
    {
        var normalizedBudgetMin = profile.BudgetMin;
        var normalizedBudgetMax = profile.BudgetMax;

        if (normalizedBudgetMin.HasValue && normalizedBudgetMax.HasValue && normalizedBudgetMin > normalizedBudgetMax)
        {
            (normalizedBudgetMin, normalizedBudgetMax) = (normalizedBudgetMax, normalizedBudgetMin);
        }

        return new GiftSearchProfile
        {
            Relationship = NormalizeTag(profile.Relationship),
            Occasion = NormalizeTag(profile.Occasion),
            BudgetMin = normalizedBudgetMin,
            BudgetMax = normalizedBudgetMax,
            InterestTags = profile.InterestTags
                .Select(NormalizeTag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            InterestFreeText = profile.InterestFreeText?.Trim(),
            Constraints = profile.Constraints?.Trim(),
            Style = NormalizeTag(profile.Style),
            ShippingTimeline = NormalizeTag(profile.ShippingTimeline),
            AgeRange = NormalizeTag(profile.AgeRange ?? string.Empty),
            AlreadyHasEverything = profile.AlreadyHasEverything
        };
    }

    private static void NormalizeSeedItem(SeedGiftIdea item)
    {
        item.Id = NormalizeTag(item.Id);
        item.Title = item.Title.Trim();
        item.Category = item.Category.Trim();
        item.MinPrice = Math.Max(0, item.MinPrice);
        item.MaxPrice = Math.Max(0, item.MaxPrice);

        if (item.MinPrice > item.MaxPrice)
        {
            (item.MinPrice, item.MaxPrice) = (item.MaxPrice, item.MinPrice);
        }

        item.RelationshipTags = NormalizeTagList(item.RelationshipTags);
        item.OccasionTags = NormalizeTagList(item.OccasionTags);
        item.InterestTags = NormalizeTagList(item.InterestTags);
        item.StyleTags = NormalizeTagList(item.StyleTags);
        item.ConstraintTags = NormalizeTagList(item.ConstraintTags);
        item.Notes = item.Notes?.Trim() ?? string.Empty;
        item.StoreSearchTerms = item.StoreSearchTerms?.Trim() ?? string.Empty;
    }

    private static List<string> NormalizeTagList(List<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeTag)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string NormalizeTag(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant();
    }

    private sealed record ScoreResult(
        SeedGiftIdea Item,
        int Score,
        bool IsRejected,
        string Rationale,
        List<string> MatchedTags);

    private sealed class SeedGiftIdea
    {
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal MinPrice { get; set; }

        public decimal MaxPrice { get; set; }

        public List<string> RelationshipTags { get; set; } = [];

        public List<string> OccasionTags { get; set; } = [];

        public List<string> InterestTags { get; set; } = [];

        public List<string> StyleTags { get; set; } = [];

        public List<string> ConstraintTags { get; set; } = [];

        public string Notes { get; set; } = string.Empty;

        public string StoreSearchTerms { get; set; } = string.Empty;
    }
}
