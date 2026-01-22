using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TendersData.Infrastructure.TendersGuru.Configuration;
using TendersData.Infrastructure.TendersGuru.Constants;
using TendersData.Infrastructure.TendersGuru.Models;
using TendersData.Infrastructure.TendersGuru.Services;

namespace TendersData.Infrastructure.Tests.TendersGuru.Services;

public abstract class TendersCacheBackgroundServiceMockHelper
{
    protected Mock<ILogger<TendersCacheBackgroundService>> LoggerMock { get; }
    protected IMemoryCache MemoryCache { get; }
    protected TendersCacheBackgroundService Service { get; }

    protected TendersCacheBackgroundServiceMockHelper()
    {
        LoggerMock = new Mock<ILogger<TendersCacheBackgroundService>>();
        MemoryCache = new MemoryCache(new MemoryCacheOptions());

        var options = Options.Create(new TendersGuruOptions { PagesCount = 1, BaseUrl = "https://tenders.guru/api/pl/" });
        var httpClient = new HttpClient(new FakeTendersGuruHttpHandler())
        {
            BaseAddress = new Uri("https://tenders.guru/api/pl/")
        };
        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider.Setup(s => s.GetService(typeof(IHttpClientFactory))).Returns(httpFactory.Object);
        scopeServiceProvider.Setup(s => s.GetService(typeof(IMemoryCache))).Returns(MemoryCache);

        var scope = new Mock<IServiceScope>();
        scope.Setup(s => s.ServiceProvider).Returns(scopeServiceProvider.Object);

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

        Service = new TendersCacheBackgroundService(
            scopeFactory.Object,
            LoggerMock.Object,
            options);
    }

    protected bool TryGetCachedTenders(out IEnumerable<TendersGuruItem>? tenders)
    {
        return MemoryCache.TryGetValue(InfrastructureConstants.CacheKeys.AllTenders, out tenders);
    }
}
