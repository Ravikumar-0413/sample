using LibraryManagementAPI.Models;
using LibraryManagementAPI.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace LibraryManagementAPI.Services;

public interface IExternalApiService
{
    Task<ExternalBookInfo?> GetBookInfoAsync(string isbn);
    Task<List<ExternalApiLog>> GetApiLogsAsync(int page = 1, int pageSize = 10);
}

public class ExternalApiService : IExternalApiService
{
    private readonly IJsonStorageService _storage;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ExternalApiService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string LogFileName = "ApiLogs";
    private const string BookInfoFileName = "ExternalBookInfo";

    public ExternalApiService(
        IJsonStorageService storage,
        IMemoryCache cache,
        ILogger<ExternalApiService> logger,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _storage = storage;
        _cache = cache;
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ExternalBookInfo?> GetBookInfoAsync(string isbn)
    {
        try
        {
            // Check cache
            var cacheKey = $"book_info_{isbn}";
            if (_cache.TryGetValue(cacheKey, out ExternalBookInfo? cachedInfo))
            {
                _logger.LogInformation($"Book info retrieved from cache for ISBN {isbn}");
                return cachedInfo;
            }

            // Check local storage
            var existingInfo = await _storage.LoadAsync<ExternalBookInfo>(BookInfoFileName);
            var stored = existingInfo.FirstOrDefault(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
            
            if (stored != null && (DateTime.UtcNow - stored.CachedAt).TotalSeconds < 
                int.Parse(_configuration["ExternalApis:CacheTTLSeconds"] ?? "3600"))
            {
                _cache.Set(cacheKey, stored, TimeSpan.FromSeconds(3600));
                _logger.LogInformation($"Book info retrieved from storage for ISBN {isbn}");
                return stored;
            }

            // Call external API
            var apiUrl = _configuration["ExternalApis:BookInfoApiUrl"]?.Replace("{isbn}", isbn);
            if (string.IsNullOrEmpty(apiUrl))
                throw new InvalidOperationException("External API URL not configured");

            var stopwatch = Stopwatch.StartNew();
            var response = await _httpClient.GetAsync(apiUrl);
            stopwatch.Stop();

            var log = new ExternalApiLog
            {
                ApiName = "OpenLibrary",
                Endpoint = apiUrl,
                RequestData = $"ISBN: {isbn}",
                StatusCode = (int)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds
            };

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                var bookInfo = new ExternalBookInfo
                {
                    Id = existingInfo.Any() ? existingInfo.Max(b => b.Id) + 1 : 1,
                    ISBN = isbn,
                    Title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "",
                    Author = root.TryGetProperty("authors", out var authorsProp) && authorsProp.ValueKind == System.Text.Json.JsonValueKind.Array
                        ? (authorsProp.EnumerateArray().FirstOrDefault().TryGetProperty("name", out var nameProp) 
                            ? nameProp.GetString() ?? "" : "")
                        : "",
                    Publisher = root.TryGetProperty("publishers", out var publishersProp) && publishersProp.ValueKind == System.Text.Json.JsonValueKind.Array
                        ? (publishersProp.EnumerateArray().FirstOrDefault().GetString() ?? "")
                        : "",
                    PublishedYear = root.TryGetProperty("publish_date", out var pubDateProp) 
                        ? int.TryParse(pubDateProp.GetString()?.Substring(Math.Max(0, pubDateProp.GetString().Length - 4)), out var year) ? year : 0 
                        : 0,
                    ApiSource = "OpenLibrary",
                    RawData = content,
                    CachedAt = DateTime.UtcNow
                };

                existingInfo.Add(bookInfo);
                await _storage.SaveAsync(BookInfoFileName, existingInfo);

                _cache.Set(cacheKey, bookInfo, TimeSpan.FromSeconds(3600));
                log.ResponseData = JsonSerializer.Serialize(bookInfo);

                await LogApiCallAsync(log);
                _logger.LogInformation($"Book info retrieved from external API for ISBN {isbn}");
                return bookInfo;
            }
            else
            {
                log.ErrorMessage = $"HTTP {response.StatusCode}";
                log.ResponseData = await response.Content.ReadAsStringAsync();
                await LogApiCallAsync(log);
                _logger.LogWarning($"External API call failed for ISBN {isbn}: {response.StatusCode}");
                return null;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP error retrieving book info for ISBN {isbn}");
            
            var log = new ExternalApiLog
            {
                ApiName = "OpenLibrary",
                Endpoint = _configuration["ExternalApis:BookInfoApiUrl"]?.Replace("{isbn}", isbn) ?? "",
                RequestData = $"ISBN: {isbn}",
                StatusCode = 0,
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
            await LogApiCallAsync(log);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book info");
            throw;
        }
    }

    public async Task<List<ExternalApiLog>> GetApiLogsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var logs = await _storage.LoadAsync<ExternalApiLog>(LogFileName);
            logs = logs.OrderByDescending(l => l.CreatedAt).ToList();
            var totalCount = logs.Count;
            logs = logs.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            _logger.LogInformation($"Retrieved {logs.Count} API logs");
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API logs");
            throw;
        }
    }

    private async Task LogApiCallAsync(ExternalApiLog log)
    {
        try
        {
            var logs = await _storage.LoadAsync<ExternalApiLog>(LogFileName);
            log.Id = logs.Any() ? logs.Max(l => l.Id) + 1 : 1;
            logs.Add(log);
            await _storage.SaveAsync(LogFileName, logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging API call");
        }
    }
}
