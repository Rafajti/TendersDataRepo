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
    ILogger<TendersDataRepository> logger) : ITendersDataRepository
{
    private const int MaxConcurrentRequests = 4;
    private readonly int pagesCount = 100;

    public async Task<IEnumerable<Tender>> GetAllTendersAsync(CancellationToken ct = default)
    {
        var semaphore = new SemaphoreSlim(MaxConcurrentRequests);

        var tasks = Enumerable.Range(1, pagesCount).Select<int, Task<IEnumerable<TendersGuruItem>>>(async page =>
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var response = await httpClient.GetFromJsonAsync<TendersGuruResponse>($"tenders?page={page}", ct);
                return response?.Data ?? [];
            }
            catch
            {
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

        return mappedTenders;
    }
}
