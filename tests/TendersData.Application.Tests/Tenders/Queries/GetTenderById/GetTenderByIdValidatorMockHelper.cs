using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public abstract class GetTenderByIdValidatorMockHelper
{
    protected GetTenderByIdValidator Validator { get; } = new();
}
