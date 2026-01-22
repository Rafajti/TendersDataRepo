using TendersData.Application.Tenders.Models;

namespace TendersData.Application.Tenders.Repositories;

public interface ITendersDataRepository
{
    Task<IEnumerable<Tender>> GetAllTendersAsync(CancellationToken ct = default);
}
