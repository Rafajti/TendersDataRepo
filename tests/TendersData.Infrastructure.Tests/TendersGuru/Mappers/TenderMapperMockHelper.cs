using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Infrastructure.TendersGuru.Mappers;

namespace TendersData.Infrastructure.Tests.TendersGuru.Mappers;

public abstract class TenderMapperMockHelper
{
    protected Mock<ILogger<TenderMapper>> LoggerMock { get; }
    protected TenderMapper Mapper { get; }

    protected TenderMapperMockHelper()
    {
        LoggerMock = new Mock<ILogger<TenderMapper>>();
        Mapper = new TenderMapper(LoggerMock.Object);
    }
}
