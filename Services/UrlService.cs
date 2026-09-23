using System.Collections.Concurrent;
using AgenticSoftwareEngineering.Models;

namespace AgenticSoftwareEngineering.Services;

public interface IUrlService
{
    Task<ShortenResponse> ShortenUrlAsync(ShortenRequest request, string baseUrl);
    Task<string?> GetLongUrlAsync(string shortCode);
    Task<AnalyticsResponse?> GetAnalyticsAsync(string shortCode);
}

public class UrlService : IUrlService
{
    private readonly ConcurrentDictionary<string, UrlMapping> _storage = new();

    public Task<ShortenResponse> ShortenUrlAsync(ShortenRequest request, string baseUrl)
    {
        if (!Uri.TryCreate(request.LongUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format.");

        string code = string.IsNullOrWhiteSpace(request.CustomAlias)
            ? Guid.NewGuid().ToString("N")[..8]
            : request.CustomAlias;

        if (_storage.ContainsKey(code))
            throw new InvalidOperationException($"Alias '{code}' is already in use.");

        var mapping = new UrlMapping { ShortCode = code, LongUrl = request.LongUrl };
        _storage[code] = mapping;

        return Task.FromResult(new ShortenResponse(code, $"{baseUrl.TrimEnd('/')}/{code}", mapping.LongUrl, mapping.CreatedAt));
    }

    public Task<string?> GetLongUrlAsync(string shortCode)
    {
        if (_storage.TryGetValue(shortCode, out var mapping))
        {
            mapping.ClickCount++;
            mapping.LastAccessedAt = DateTime.UtcNow;
            return Task.FromResult<string?>(mapping.LongUrl);
        }
        return Task.FromResult<string?>(null);
    }

    public Task<AnalyticsResponse?> GetAnalyticsAsync(string shortCode)
    {
        if (_storage.TryGetValue(shortCode, out var mapping))
        {
            return Task.FromResult<AnalyticsResponse?>(
                new AnalyticsResponse(mapping.ShortCode, mapping.LongUrl, mapping.ClickCount, mapping.CreatedAt, mapping.LastAccessedAt));
        }
        return Task.FromResult<AnalyticsResponse?>(null);
    }
}