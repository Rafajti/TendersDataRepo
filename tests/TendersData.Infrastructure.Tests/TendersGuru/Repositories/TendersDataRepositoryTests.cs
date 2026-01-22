using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Constants;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;
using TendersData.Infrastructure.TendersGuru.Repositories;

namespace TendersData.Infrastructure.Tests.TendersGuru.Repositories;

public class TendersDataRepositoryTests
{
    private readonly Mock<ILogger<TendersDataRepository>> _loggerMock;
    private readonly Mock<ITenderMapper> _mapperMock;
    private readonly IMemoryCache _memoryCache;
    private readonly TendersDataRepository _repository;

    public TendersDataRepositoryTests()
    {
        _loggerMock = new Mock<ILogger<TendersDataRepository>>();
        _mapperMock = new Mock<ITenderMapper>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _repository = new TendersDataRepository(_loggerMock.Object, _memoryCache, _mapperMock.Object);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithCachedData_ReturnsCachedTenders()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            new TendersGuruItem { Id = "1", Date = "2024-01-01", Title = "Tender 1", AmountEur = "100" },
            new TendersGuruItem { Id = "2", Date = "2024-01-02", Title = "Tender 2", AmountEur = "200" }
        };

        var expectedTenders = new List<Tender>
        {
            new Tender(1, DateTime.Parse("2024-01-01"), "Tender 1", "", 100m, new List<Supplier>()),
            new Tender(2, DateTime.Parse("2024-01-02"), "Tender 2", "", 200m, new List<Supplier>())
        };

        // Simulate background service loading data into cache
        _memoryCache.Set(TendersCacheKeys.AllTenders, cachedItems);
        _mapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenders);
        _mapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithEmptyCache_ReturnsEmptyList()
    {
        // Arrange - cache is empty (background service hasn't loaded data yet)

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mapperMock.Verify(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithNullCachedData_ReturnsEmptyList()
    {
        // Arrange
        _memoryCache.Set(TendersCacheKeys.AllTenders, (IEnumerable<TendersGuruItem>?)null);
        _mapperMock.Setup(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>())).Returns(Enumerable.Empty<Tender>());

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTendersAsync_WithCache_ReturnsCachedDataOnMultipleCalls()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            new TendersGuruItem { Id = "1", Date = "2024-01-01", Title = "Tender 1", AmountEur = "100" }
        };

        var expectedTenders = new List<Tender>
        {
            new Tender(1, DateTime.Parse("2024-01-01"), "Tender 1", "", 100m, new List<Supplier>())
        };

        _memoryCache.Set(TendersCacheKeys.AllTenders, cachedItems);
        _mapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);

        // Act - Multiple calls should all return cached data
        var firstResult = await _repository.GetAllTendersAsync();
        var secondResult = await _repository.GetAllTendersAsync();
        var thirdResult = await _repository.GetAllTendersAsync();

        // Assert
        firstResult.Should().NotBeNull();
        secondResult.Should().NotBeNull();
        thirdResult.Should().NotBeNull();
        firstResult.Should().BeEquivalentTo(secondResult);
        secondResult.Should().BeEquivalentTo(thirdResult);
        firstResult.Should().BeEquivalentTo(expectedTenders);
        _mapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Exactly(3));
    }

    [Fact]
    public async Task GetAllTendersAsync_WithCancellationToken_AcceptsToken()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            new TendersGuruItem { Id = "1", Date = "2024-01-01", Title = "Tender 1", AmountEur = "100" }
        };

        var expectedTenders = new List<Tender>
        {
            new Tender(1, DateTime.Parse("2024-01-01"), "Tender 1", "", 100m, new List<Supplier>())
        };

        _memoryCache.Set(TendersCacheKeys.AllTenders, cachedItems);
        _mapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var result = await _repository.GetAllTendersAsync(cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenders);
        _mapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_AfterCacheExpiration_ReturnsEmptyList()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            new TendersGuruItem { Id = "1", Date = "2024-01-01", Title = "Tender 1", AmountEur = "100" }
        };

        // Set cache with expiration that has already passed
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(-1) // Already expired
        };
        _memoryCache.Set(TendersCacheKeys.AllTenders, cachedItems, cacheOptions);

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert - Cache entry expired, so should return empty
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mapperMock.Verify(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()), Times.Never);
    }
}
