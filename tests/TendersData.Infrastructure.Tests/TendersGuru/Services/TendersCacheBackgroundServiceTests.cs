using FluentAssertions;
using TendersData.Infrastructure.TendersGuru.Models;

namespace TendersData.Infrastructure.Tests.TendersGuru.Services;

public class TendersCacheBackgroundServiceTests : TendersCacheBackgroundServiceMockHelper
{
    [Fact]
    public async Task ExecuteAsync_AfterFirstLoad_PopulatesCacheWithTenders()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var runTask = Service.StartAsync(cts.Token);

        await Task.Delay(2500);
        await cts.CancelAsync();
        await runTask;

        // Assert
        TryGetCachedTenders(out var tenders).Should().BeTrue();
        tenders.Should().NotBeNull();
        var list = tenders!.ToList();
        list.Should().NotBeEmpty();
        list[0].Id.Should().Be("1");
        list[0].Title.Should().Be("Unit Test Tender");
        list[0].AmountEur.Should().Be("5000");
    }
}
