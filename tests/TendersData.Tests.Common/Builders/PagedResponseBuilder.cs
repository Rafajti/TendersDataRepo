using TendersData.Application.Tenders.Models;

namespace TendersData.Tests.Common.Builders;

public sealed class PagedResponseBuilder<T>
{
    private IEnumerable<T> _data = [];
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private int _totalCount;

    public PagedResponseBuilder<T> WithData(IEnumerable<T> data)
    {
        _data = data.ToList();
        return this;
    }

    public PagedResponseBuilder<T> WithData(params T[] data)
    {
        _data = data;
        return this;
    }

    public PagedResponseBuilder<T> WithPageNumber(int pageNumber)
    {
        _pageNumber = pageNumber;
        return this;
    }

    public PagedResponseBuilder<T> WithPageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    public PagedResponseBuilder<T> WithTotalCount(int totalCount)
    {
        _totalCount = totalCount;
        return this;
    }

    public PagedResponse<T> Build() => new()
    {
        Data = _data,
        PageNumber = _pageNumber,
        PageSize = _pageSize,
        TotalCount = _totalCount
    };

    public static PagedResponseBuilder<T> Default => new();
}
