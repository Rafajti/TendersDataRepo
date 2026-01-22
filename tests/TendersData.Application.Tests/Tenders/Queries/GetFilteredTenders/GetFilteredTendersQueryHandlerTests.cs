using FluentAssertions;
using Moq;
using TendersData.Application.Tenders;
using TendersData.Tests.Common.Builders;

namespace TendersData.Application.Tests.Tenders.Queries.GetFilteredTenders;

public class GetFilteredTendersQueryHandlerTests : GetFilteredTendersQueryHandlerMockHelper
{
    [Fact]
    public async Task Handle_WithNoFilters_ReturnsPagedTenders()
    {
        // Arrange
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithTitle("T1").WithAmountEur(100m).Build(),
            TenderBuilder.Default.WithId(2).WithTitle("T2").WithAmountEur(200m).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default.WithPageNumber(1).WithPageSize(10).Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).Build(),
            TenderBuilder.Default.WithId(2).Build(),
            TenderBuilder.Default.WithId(3).Build(),
            TenderBuilder.Default.WithId(4).Build(),
            TenderBuilder.Default.WithId(5).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithPageNumber(2)
            .WithPageSize(2)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinPriceEurAndMaxPriceEur_FiltersByPrice()
    {
        // Arrange
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithAmountEur(500m).Build(),
            TenderBuilder.Default.WithId(2).WithAmountEur(1500m).Build(),
            TenderBuilder.Default.WithId(3).WithAmountEur(2500m).Build(),
            TenderBuilder.Default.WithId(4).WithAmountEur(3500m).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithMinPriceEur(1000)
            .WithMaxPriceEur(3000)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Data!.Select(t => t.AmountEur).Should().OnlyContain(x => x >= 1000 && x <= 3000);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithDateFromAndDateTo_FiltersByDate()
    {
        // Arrange
        var from = new DateTime(2024, 5, 1);
        var to = new DateTime(2024, 6, 30);
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithDate(new DateTime(2024, 4, 15)).Build(),
            TenderBuilder.Default.WithId(2).WithDate(new DateTime(2024, 5, 15)).Build(),
            TenderBuilder.Default.WithId(3).WithDate(new DateTime(2024, 6, 15)).Build(),
            TenderBuilder.Default.WithId(4).WithDate(new DateTime(2024, 7, 15)).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithDateFrom(from)
            .WithDateTo(to)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        foreach (var t in result.Data!)
        {
            t.Date.Should().BeOnOrAfter(from);
            t.Date.Should().BeOnOrBefore(to);
        }
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithSupplierId_FiltersBySupplier()
    {
        // Arrange
        var sup1 = new SupplierBuilder().WithId(1).WithName("S1").Build();
        var sup2 = new SupplierBuilder().WithId(2).WithName("S2").Build();
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithSuppliers(sup1).Build(),
            TenderBuilder.Default.WithId(2).WithSuppliers(sup2).Build(),
            TenderBuilder.Default.WithId(3).WithSuppliers(sup1, sup2).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithSupplierId(2)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Data!.All(t => t.Suppliers.Any(s => s.Id == 2)).Should().BeTrue();
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithSortByPriceAsc_ReturnsOrderedByAmount()
    {
        // Arrange
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithAmountEur(300m).Build(),
            TenderBuilder.Default.WithId(2).WithAmountEur(100m).Build(),
            TenderBuilder.Default.WithId(3).WithAmountEur(200m).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithSortBy(TendersConstants.SortBy.Price)
            .WithSortOrder(TendersConstants.SortOrder.Ascending)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        var list = result.Data!.ToList();
        list.Should().HaveCount(3);
        list[0].AmountEur.Should().Be(100m);
        list[1].AmountEur.Should().Be(200m);
        list[2].AmountEur.Should().Be(300m);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithSortByPriceDesc_ReturnsOrderedByAmountDescending()
    {
        // Arrange
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithAmountEur(100m).Build(),
            TenderBuilder.Default.WithId(2).WithAmountEur(300m).Build(),
            TenderBuilder.Default.WithId(3).WithAmountEur(200m).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithSortBy(TendersConstants.SortBy.Price)
            .WithSortOrder(TendersConstants.SortOrder.Descending)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        var list = result.Data!.ToList();
        list.Should().HaveCount(3);
        list[0].AmountEur.Should().Be(300m);
        list[1].AmountEur.Should().Be(200m);
        list[2].AmountEur.Should().Be(100m);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithSortByDateAsc_ReturnsOrderedByDate()
    {
        // Arrange
        var d1 = new DateTime(2024, 6, 1);
        var d2 = new DateTime(2024, 5, 1);
        var d3 = new DateTime(2024, 7, 1);
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithDate(d1).Build(),
            TenderBuilder.Default.WithId(2).WithDate(d2).Build(),
            TenderBuilder.Default.WithId(3).WithDate(d3).Build()
        };
        SetupGetAllTendersAsync(tenders);
        var query = GetFilteredTendersQueryBuilder.Default
            .WithSortBy(TendersConstants.SortBy.Date)
            .WithSortOrder(TendersConstants.SortOrder.Ascending)
            .WithPageNumber(1)
            .WithPageSize(10)
            .Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        var list = result.Data!.ToList();
        list.Should().HaveCount(3);
        list[0].Date.Should().Be(d2);
        list[1].Date.Should().Be(d1);
        list[2].Date.Should().Be(d3);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ReturnsEmptyPagedResponse()
    {
        // Arrange
        SetupGetAllTendersAsync([]);
        var query = GetFilteredTendersQueryBuilder.Default.Build();
        var ct = CancellationToken.None;

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesItToRepository()
    {
        // Arrange
        SetupGetAllTendersAsync([]);
        var query = GetFilteredTendersQueryBuilder.Default.Build();
        var ct = new CancellationTokenSource().Token;

        // Act
        await Handler.Handle(query, ct);

        // Assert
        RepositoryMock.Verify(r => r.GetAllTendersAsync(ct), Moq.Times.Once);
    }
}
