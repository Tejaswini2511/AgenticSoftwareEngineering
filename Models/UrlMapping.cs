namespace AgenticSoftwareEngineering.Models;

public record ShortenRequest(string LongUrl, string? CustomAlias = null);
public record ShortenResponse(string ShortCode, string ShortUrl, string LongUrl, DateTime CreatedAt);
public record AnalyticsResponse(string ShortCode, string LongUrl, int ClickCount, DateTime CreatedAt, DateTime? LastAccessedAt);

public class UrlMapping
{
    public string ShortCode { get; set; } = string.Empty;
    public string LongUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ClickCount { get; set; } = 0;
    public DateTime? LastAccessedAt { get; set; }
}