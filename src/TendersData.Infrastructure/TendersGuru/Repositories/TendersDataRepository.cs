using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Repositories;
using TendersData.Infrastructure.TendersGuru.Constants;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Infrastructure.TendersGuru.Repositories;

public class TendersDataRepository(
    ILogger<TendersDataRepository> logger,
    IMemoryCache memoryCache,
    ITenderMapper mapper) : ITendersDataRepository
{
    public async Task<IEnumerable<Tender>> GetAllTendersAsync(CancellationToken ct = default)
    {
        if (memoryCache.TryGetValue<IEnumerable<TendersGuruItem>>(TendersCacheKeys.AllTenders, out var cachedApiData))
        {
            logger.LogInformation("Returning cached tenders.");
            var mappedTenders = mapper.MapToDomain(cachedApiData ?? []);
            return mappedTenders;
        }

        logger.LogWarning("Cache is empty - data not yet loaded by background service");
        return [];
    }
}
