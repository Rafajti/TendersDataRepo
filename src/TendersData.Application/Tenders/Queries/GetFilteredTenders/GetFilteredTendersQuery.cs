using MediatR;
using TendersData.Application.Tenders.Models;

namespace TendersData.Application.Tenders.Queries.GetFilteredTenders;

public record GetFilteredTendersQuery : IRequest<PagedResponse<Tender>>
{
    public decimal? MinPriceEur { get; init; }
    public decimal? MaxPriceEur { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public int? SupplierId { get; init; }
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = TendersConstants.DefaultPageSize;
}
