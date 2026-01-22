using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Repositories;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Infrastructure.TendersGuru.Repositories;

public class TendersDataRepository(
    HttpClient httpClient,
    ITenderMapper mapper,
    ILogger<TendersDataRepository> logger,
    IMemoryCache memoryCache) : ITendersDataRepository
{
    private const int MaxConcurrentRequests = 4;
    private readonly int pagesCount = 10;
    private const string CacheKey = "tenders:all";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);

    public async Task<IEnumerable<Tender>> GetAllTendersAsync(CancellationToken ct = default)
    {
        if (memoryCache.TryGetValue<IEnumerable<Tender>>(CacheKey, out var cachedTenders))
        {
            logger.LogInformation("Returning cached tenders");
            return cachedTenders ?? Enumerable.Empty<Tender>();
        }

        var semaphore = new SemaphoreSlim(MaxConcurrentRequests);

        var tasks = Enumerable.Range(1, pagesCount).Select<int, Task<IEnumerable<TendersGuruItem>>>(async page =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var response = await httpClient.GetFromJsonAsync<TendersGuruResponse>($"tenders?page={page}", ct);
                return response?.Data ?? [];
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning("Failed to fetch page {Page} after retries: {Error}", page, ex.Message);
                return Array.Empty<TendersGuruItem>();
            }
            catch (TaskCanceledException ex)
            {
                logger.LogWarning("Timeout for page {Page} after retries: {Error}", page, ex.Message);
                return Array.Empty<TendersGuruItem>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error fetching page {Page}", page);
                return Array.Empty<TendersGuruItem>();
            }
            finally
            {
                semaphore.Release();
            }
        });

        var pageResults = await Task.WhenAll(tasks);
        var responseTenders = pageResults.SelectMany(x => x).ToList();
        var mappedTenders = mapper.MapToDomain(responseTenders);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration
        };

        memoryCache.Set(CacheKey, mappedTenders, cacheOptions);
        logger.LogInformation("Cached tenders with expiration of {Expiration} minutes", CacheExpiration.TotalMinutes);

        return mappedTenders;
    }
}
