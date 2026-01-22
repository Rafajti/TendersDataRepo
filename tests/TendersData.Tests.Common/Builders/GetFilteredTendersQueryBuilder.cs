using TendersData.Application.Tenders.Queries.GetFilteredTenders;
using static TendersData.Application.Tenders.TendersConstants;

namespace TendersData.Tests.Common.Builders;

public sealed class GetFilteredTendersQueryBuilder
{
    private decimal? _minPriceEur;
    private decimal? _maxPriceEur;
    private DateTime? _dateFrom;
    private DateTime? _dateTo;
    private int? _supplierId;
    private string? _sortBy;
    private string? _sortOrder;
    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public GetFilteredTendersQueryBuilder WithMinPriceEur(decimal? value)
    {
        _minPriceEur = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithMaxPriceEur(decimal? value)
    {
        _maxPriceEur = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithDateFrom(DateTime? value)
    {
        _dateFrom = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithDateTo(DateTime? value)
    {
        _dateTo = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithSupplierId(int? value)
    {
        _supplierId = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithSortBy(string? value)
    {
        _sortBy = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithSortOrder(string? value)
    {
        _sortOrder = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithPageNumber(int value)
    {
        _pageNumber = value;
        return this;
    }

    public GetFilteredTendersQueryBuilder WithPageSize(int value)
    {
        _pageSize = value;
        return this;
    }

    public GetFilteredTendersQuery Build() => new()
    {
        MinPriceEur = _minPriceEur,
        MaxPriceEur = _maxPriceEur,
        DateFrom = _dateFrom,
        DateTo = _dateTo,
        SupplierId = _supplierId,
        SortBy = _sortBy,
        SortOrder = _sortOrder,
        PageNumber = _pageNumber,
        PageSize = _pageSize
    };

    public static GetFilteredTendersQueryBuilder Default => new();
}
