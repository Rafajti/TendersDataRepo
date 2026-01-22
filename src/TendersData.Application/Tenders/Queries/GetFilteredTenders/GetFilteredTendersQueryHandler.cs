using MediatR;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Repositories;

namespace TendersData.Application.Tenders.Queries.GetFilteredTenders;

public class GetFilteredTendersQueryHandler(ITendersDataRepository tendersRepository) : IRequestHandler<GetFilteredTendersQuery, PagedResponse<Tender>>
{
    public async Task<PagedResponse<Tender>> Handle(GetFilteredTendersQuery request, CancellationToken ct)
    {
        var allTenders = await tendersRepository.GetAllTendersAsync(ct);

        allTenders = ApplyPriceFilters(allTenders, request.MinPriceEur, request.MaxPriceEur);
        allTenders = ApplyDateFilters(allTenders, request.DateFrom, request.DateTo);
        allTenders = ApplySupplierFilter(allTenders, request.SupplierId);

        var sorted = ApplySorting(allTenders, request.SortBy, request.SortOrder);

        var totalCount = sorted.Count();
        var pagedData = sorted
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResponse<Tender>
        {
            Data = pagedData,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IEnumerable<Tender> ApplyPriceFilters(
        IEnumerable<Tender> query,
        decimal? minPriceEur,
        decimal? maxPriceEur)
    {
        if (minPriceEur.HasValue)
        {
            query = query.Where(t => t.AmountEur >= minPriceEur.Value);
        }

        if (maxPriceEur.HasValue)
        {
            query = query.Where(t => t.AmountEur <= maxPriceEur.Value);
        }

        return query;
    }

    private static IEnumerable<Tender> ApplyDateFilters(
        IEnumerable<Tender> query,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        if (dateFrom.HasValue)
        {
            query = query.Where(t => t.Date >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(t => t.Date <= dateTo.Value);
        }

        return query;
    }

    private static IEnumerable<Tender> ApplySupplierFilter(
        IEnumerable<Tender> query,
        int? supplierId)
    {
        if (supplierId.HasValue)
        {
            query = query.Where(t => t.Suppliers.Any(s => s.Id == supplierId.Value));
        }

        return query;
    }

    private static IEnumerable<Tender> ApplySorting(
        IEnumerable<Tender> query,
        string? sortBy,
        string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            return query;
        }

        var isAscending = sortOrder?.ToLower() == TendersConstants.SortOrder.Ascending;
        var sortByLower = sortBy.ToLower();

        return sortByLower switch
        {
            TendersConstants.SortBy.Price => isAscending
                ? query.OrderBy(t => t.AmountEur)
                : query.OrderByDescending(t => t.AmountEur),
            TendersConstants.SortBy.Date => isAscending
                ? query.OrderBy(t => t.Date)
                : query.OrderByDescending(t => t.Date),
            _ => query
        };
    }
}