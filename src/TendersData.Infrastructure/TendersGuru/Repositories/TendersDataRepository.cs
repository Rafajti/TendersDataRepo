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
    private readonly int pagesCount = 5;
    public async Task<IEnumerable<Tender>> GetAllTendersAsync(CancellationToken ct = default)
    {
        var allTenders = new List<Tender>();
        var responseTenders = new List<TendersGuruItem>();

        for (int i = 1; i <= pagesCount; i++)
        {
            try
            {
                var response = await httpClient.GetFromJsonAsync<TendersGuruResponse>($"tenders?page={i}", ct);

                if (response?.Data == null) continue;
                responseTenders.AddRange(response.Data);
            }
            catch
            {
                logger.LogWarning("Cos sie wypieprzylo");
            }
        }

        var mappedTenders = mapper.MapToDomain(responseTenders);
        allTenders.AddRange(mappedTenders);

        return allTenders;
    }
}
