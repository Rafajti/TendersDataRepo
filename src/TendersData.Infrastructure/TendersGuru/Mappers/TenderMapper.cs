using Microsoft.Extensions.Logging;
using System.Globalization;
using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Infrastructure.TendersGuru.Mappers;

public class TenderMapper(ILogger<TenderMapper> logger) : ITenderMapper
{
    public IEnumerable<Tender> MapToDomain(IEnumerable<TendersGuruItem> items)
    {
        return items.Select(MapToDomain);
    }

    private Tender MapToDomain(TendersGuruItem item)
    {
        if (!int.TryParse(item.Id, out var id))
        {
            logger.LogWarning("Incorrect ID tender: {Id}", item.Id);
        }

        var amountEur = decimal.TryParse(
            item.AmountEur,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out var price)
            ? price
            : 0;

        var date = DateTime.TryParse(item.Date, out var parsedDate)
            ? parsedDate
            : DateTime.MinValue;

        var suppliers = item.Suppliers?
            .Select(s => new Supplier(s.Id, s.Name ?? string.Empty))
            .ToList() ?? [];

        return new Tender(
            Id: id,
            Date: date,
            Title: item.Title ?? string.Empty,
            Description: item.Description ?? string.Empty,
            AmountEur: amountEur,
            Suppliers: suppliers
        );
    }
}
