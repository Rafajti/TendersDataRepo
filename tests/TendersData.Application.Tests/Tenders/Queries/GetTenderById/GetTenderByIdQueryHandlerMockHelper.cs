using Moq;
using TendersData.Application.Tenders.Queries.GetTenderById;
using TendersData.Application.Tenders.Repositories;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public abstract class GetTenderByIdQueryHandlerMockHelper
{
    protected Mock<ITendersDataRepository> RepositoryMock { get; }
    protected GetTenderByIdQueryHandler Handler { get; }

    protected GetTenderByIdQueryHandlerMockHelper()
    {
        RepositoryMock = new Mock<ITendersDataRepository>();
        Handler = new GetTenderByIdQueryHandler(RepositoryMock.Object);
    }

    protected void SetupGetAllTendersAsync(IEnumerable<TendersData.Application.Tenders.Models.Tender> tenders)
    {
        RepositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenders);
    }

    protected void SetupGetAllTendersAsync(CancellationToken ct, IEnumerable<TendersData.Application.Tenders.Models.Tender> tenders)
    {
        RepositoryMock
            .Setup(r => r.GetAllTendersAsync(ct))
            .ReturnsAsync(tenders);
    }

    protected void VerifyGetAllTendersAsync(Times times)
    {
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            times);
    }

    protected void VerifyGetAllTendersAsync(CancellationToken ct, Times times)
    {
        RepositoryMock.Verify(r => r.GetAllTendersAsync(ct), times);
    }
}
