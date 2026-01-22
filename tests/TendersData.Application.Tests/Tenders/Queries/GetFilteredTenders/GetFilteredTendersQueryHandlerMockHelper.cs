using Moq;
using TendersData.Application.Tenders.Queries.GetFilteredTenders;
using TendersData.Application.Tenders.Repositories;

namespace TendersData.Application.Tests.Tenders.Queries.GetFilteredTenders;

public abstract class GetFilteredTendersQueryHandlerMockHelper
{
    protected Mock<ITendersDataRepository> RepositoryMock { get; }
    protected GetFilteredTendersQueryHandler Handler { get; }

    protected GetFilteredTendersQueryHandlerMockHelper()
    {
        RepositoryMock = new Mock<ITendersDataRepository>();
        Handler = new GetFilteredTendersQueryHandler(RepositoryMock.Object);
    }

    protected void SetupGetAllTendersAsync(IEnumerable<TendersData.Application.Tenders.Models.Tender> tenders)
    {
        RepositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenders);
    }

    protected void VerifyGetAllTendersAsync(Times times)
    {
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            times);
    }
}
