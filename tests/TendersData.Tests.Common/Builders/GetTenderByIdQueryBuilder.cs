using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Tests.Common.Builders;

public sealed class GetTenderByIdQueryBuilder
{
    private int _id = 1;

    public GetTenderByIdQueryBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public GetTenderByIdQuery Build() => new(_id);

    public static GetTenderByIdQueryBuilder Default => new();
}
