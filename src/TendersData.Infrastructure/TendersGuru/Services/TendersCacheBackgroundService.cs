using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using TendersData.Infrastructure.TendersGuru.Configuration;
using TendersData.Infrastructure.TendersGuru.Constants;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;
using TendersData.Infrastructure.TendersGuru.Repositories;

namespace TendersData.Infrastructure.TendersGuru.Services;

public class TendersCacheBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TendersCacheBackgroundService> logger,
    IOptions<TendersGuruOptions> options) : BackgroundService
{
    private const int MaxConcurrentRequests = 4;
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromHours(1);
    private readonly TendersGuruOptions _options = options.Value;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TendersCacheBackgroundService is starting");

        await LoadTendersAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_refreshInterval, stoppingToken);
            
            if (!stoppingToken.IsCancellationRequested)
            {
                await LoadTendersAsync(stoppingToken);
            }
        }

        logger.LogInformation("TendersCacheBackgroundService is stopping");
    }

    private async Task LoadTendersAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Refreshing tenders cache");
            
            using var scope = serviceScopeFactory.CreateScope();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(TendersDataRepository));
            var mapper = scope.ServiceProvider.GetRequiredService<ITenderMapper>();
            var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            
            var semaphore = new SemaphoreSlim(MaxConcurrentRequests);

            var tasks = Enumerable.Range(1, _options.PagesCount).Select<int, Task<IEnumerable<TendersGuruItem>>>(async page =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var response = await httpClient.GetFromJsonAsync<TendersGuruResponse>($"tenders?page={page}", cancellationToken);
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
            var responseTenders = pageResults.SelectMany(x => x);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiration
            };

            memoryCache.Set(TendersCacheKeys.AllTenders, responseTenders, cacheOptions);
            logger.LogInformation("Loaded and cached {Count} tenders with expiration of {Expiration} minutes",
                responseTenders.Count(), CacheExpiration.TotalMinutes);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Cache refresh was cancelled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while refreshing tenders cache");
        }
    }
}
