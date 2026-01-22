using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Repositories;

namespace TendersData.Infrastructure.Tests.TendersGuru.Repositories;

public abstract class TendersDataRepositoryMockHelper
{
    protected Mock<ILogger<TendersDataRepository>> LoggerMock { get; }
    protected Mock<ITenderMapper> MapperMock { get; }
    protected IMemoryCache MemoryCache { get; }
    protected TendersDataRepository Repository { get; }

    protected TendersDataRepositoryMockHelper()
    {
        LoggerMock = new Mock<ILogger<TendersDataRepository>>();
        MapperMock = new Mock<ITenderMapper>();
        MemoryCache = new MemoryCache(new MemoryCacheOptions());
        Repository = new TendersDataRepository(LoggerMock.Object, MemoryCache, MapperMock.Object);
    }
}
