using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Infrastructure.TendersGuru.Mappers;

public interface ITenderMapper
{
    IEnumerable<Tender> MapToDomain(IEnumerable<TendersGuruItem> items);
}
