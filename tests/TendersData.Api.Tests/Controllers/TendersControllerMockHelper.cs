using MediatR;
using Moq;
using TendersData.Api.Controllers;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Api.Tests.Controllers;

public abstract class TendersControllerMockHelper
{
    protected Mock<IMediator> MediatorMock { get; }
    protected TendersController Controller { get; }

    protected TendersControllerMockHelper()
    {
        MediatorMock = new Mock<IMediator>();
        Controller = new TendersController(MediatorMock.Object);
    }

    protected void SetupGetById(int id, Tender? tender)
    {
        MediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tender);
    }

    protected void SetupGetByIdThrows(int id, Exception ex)
    {
        MediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ex);
    }

    protected void VerifyGetByIdSent(int id, Times times)
    {
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            times);
    }
}
