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
    private readonly TendersGuruOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("TendersCacheBackgroundService is starting");

        await LoadTendersAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(InfrastructureConstants.BackgroundService.RefreshIntervalMinutes, stoppingToken);
            
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
            var memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
            
            var allTenders = await FetchAllPagesAsync(httpClient, cancellationToken);
            
            var cacheOptions = BuildCacheEntryOptions();
            SetCache(memoryCache, allTenders, cacheOptions);
            
            logger.LogInformation("Loaded and cached {Count} tenders with expiration of {Expiration} minutes",
                allTenders.Count(), InfrastructureConstants.BackgroundService.CacheExpiration.TotalMinutes);
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

    private async Task<IEnumerable<TendersGuruItem>> FetchAllPagesAsync(HttpClient client, CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(InfrastructureConstants.BackgroundService.MaxConcurrentRequests);
        
        var tasks = Enumerable.Range(1, _options.PagesCount).Select(page => 
            FetchPageAsync(client, page, semaphore, cancellationToken));

        var pageResults = await Task.WhenAll(tasks);
        return pageResults.SelectMany(x => x);
    }

    private async Task<IEnumerable<TendersGuruItem>> FetchPageAsync(
        HttpClient client, 
        int page, 
        SemaphoreSlim semaphore, 
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var response = await client.GetFromJsonAsync<TendersGuruResponse>($"tenders?page={page}", cancellationToken);
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
    }

    private static MemoryCacheEntryOptions BuildCacheEntryOptions()
    {
        return new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = InfrastructureConstants.BackgroundService.CacheExpiration
        };
    }

    private static void SetCache(
        IMemoryCache memoryCache, 
        IEnumerable<TendersGuruItem> tenders, 
        MemoryCacheEntryOptions cacheOptions)
    {
        memoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, tenders, cacheOptions);
    }
}
