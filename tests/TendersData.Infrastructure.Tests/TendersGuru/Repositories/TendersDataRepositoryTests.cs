using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Constants;
using TendersData.Infrastructure.TendersGuru.Models;
using TendersData.Tests.Common.Builders;

namespace TendersData.Infrastructure.Tests.TendersGuru.Repositories;

public class TendersDataRepositoryTests : TendersDataRepositoryMockHelper
{
    [Fact]
    public async Task GetAllTendersAsync_WithCachedData_ReturnsCachedTenders()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            TendersGuruItemBuilder.Default.WithId("1").WithDate("2024-01-01").WithTitle("Tender 1").WithAmountEur("100").Build(),
            TendersGuruItemBuilder.Default.WithId("2").WithDate("2024-01-02").WithTitle("Tender 2").WithAmountEur("200").Build()
        };
        var expectedTenders = new List<Tender>
        {
            TenderBuilder.Default.WithId(1).WithDate(DateTime.Parse("2024-01-01")).WithTitle("Tender 1").WithAmountEur(100m).Build(),
            TenderBuilder.Default.WithId(2).WithDate(DateTime.Parse("2024-01-02")).WithTitle("Tender 2").WithAmountEur(200m).Build()
        };
        MemoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, cachedItems);
        MapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);

        // Act
        var result = await Repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenders);
        MapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithEmptyCache_ReturnsEmptyList()
    {
        // Arrange – cache jest pusty

        // Act
        var result = await Repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        MapperMock.Verify(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()), Times.Never);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithNullCachedData_ReturnsEmptyList()
    {
        // Arrange
        MemoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, (IEnumerable<TendersGuruItem>?)null);
        MapperMock.Setup(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>())).Returns(Enumerable.Empty<Tender>());

        // Act
        var result = await Repository.GetAllTendersAsync();

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
            TendersGuruItemBuilder.Default.WithId("1").WithDate("2024-01-01").WithTitle("Tender 1").WithAmountEur("100").Build()
        };
        var expectedTenders = new List<Tender>
        {
            TenderBuilder.Default.WithId(1).WithDate(DateTime.Parse("2024-01-01")).WithTitle("Tender 1").WithAmountEur(100m).Build()
        };
        MemoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, cachedItems);
        MapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);

        // Act
        var firstResult = await Repository.GetAllTendersAsync();
        var secondResult = await Repository.GetAllTendersAsync();
        var thirdResult = await Repository.GetAllTendersAsync();

        // Assert
        firstResult.Should().NotBeNull();
        secondResult.Should().NotBeNull();
        thirdResult.Should().NotBeNull();
        firstResult.Should().BeEquivalentTo(secondResult);
        secondResult.Should().BeEquivalentTo(thirdResult);
        firstResult.Should().BeEquivalentTo(expectedTenders);
        MapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Exactly(3));
    }

    [Fact]
    public async Task GetAllTendersAsync_WithCancellationToken_AcceptsToken()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            TendersGuruItemBuilder.Default.WithId("1").WithDate("2024-01-01").WithTitle("Tender 1").WithAmountEur("100").Build()
        };
        var expectedTenders = new List<Tender>
        {
            TenderBuilder.Default.WithId(1).WithDate(DateTime.Parse("2024-01-01")).WithTitle("Tender 1").WithAmountEur(100m).Build()
        };
        MemoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, cachedItems);
        MapperMock.Setup(m => m.MapToDomain(cachedItems)).Returns(expectedTenders);
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var result = await Repository.GetAllTendersAsync(cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenders);
        MapperMock.Verify(m => m.MapToDomain(cachedItems), Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_AfterCacheExpiration_ReturnsEmptyList()
    {
        // Arrange
        var cachedItems = new List<TendersGuruItem>
        {
            TendersGuruItemBuilder.Default.WithId("1").WithDate("2024-01-01").WithTitle("Tender 1").WithAmountEur("100").Build()
        };
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(50)
        };
        MemoryCache.Set(InfrastructureConstants.CacheKeys.AllTenders, cachedItems, cacheOptions);
        await Task.Delay(100);

        // Act
        var result = await Repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        MapperMock.Verify(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()), Times.Never);
    }
}
