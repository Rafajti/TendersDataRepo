using FluentAssertions;
using Moq;
using TendersData.Tests.Common.Builders;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public class GetTenderByIdQueryHandlerTests : GetTenderByIdQueryHandlerMockHelper
{
    [Fact]
    public async Task Handle_WithValidId_ReturnsTender()
    {
        // Arrange
        var id = 1;
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();
        var ct = CancellationToken.None;
        var expectedTender = TenderBuilder.Default
            .WithId(id)
            .WithTitle("Test Tender")
            .WithDescription("Test Description")
            .WithAmountEur(1000.50m)
            .WithSupplier(1, "Supplier 1")
            .Build();
        SetupGetAllTendersAsync([expectedTender]);

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTender);
        result!.Id.Should().Be(id);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var id = 999;
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();
        var ct = CancellationToken.None;
        SetupGetAllTendersAsync([]);

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleTenders_ReturnsCorrectTender()
    {
        // Arrange
        var id = 2;
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();
        var ct = CancellationToken.None;
        var tenders = new[]
        {
            TenderBuilder.Default.WithId(1).WithTitle("Tender 1").WithAmountEur(100m).Build(),
            TenderBuilder.Default.WithId(2).WithTitle("Tender 2").WithAmountEur(200m).Build(),
            TenderBuilder.Default.WithId(3).WithTitle("Tender 3").WithAmountEur(300m).Build()
        };
        SetupGetAllTendersAsync(tenders);

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Title.Should().Be("Tender 2");
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task Handle_WithDifferentIds_CallsRepository(int id)
    {
        // Arrange
        var query = GetTenderByIdQueryBuilder.Default.WithId(id).Build();
        var ct = CancellationToken.None;
        var tenders = new[] { TenderBuilder.Default.WithId(id).WithTitle($"Tender {id}").WithAmountEur(500m).Build() };
        SetupGetAllTendersAsync(tenders);

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        RepositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesItToRepository()
    {
        // Arrange
        var query = GetTenderByIdQueryBuilder.Default.WithId(1).Build();
        var cts = new CancellationTokenSource();
        var ct = cts.Token;
        SetupGetAllTendersAsync(ct, []);

        // Act
        await Handler.Handle(query, ct);

        // Assert
        VerifyGetAllTendersAsync(ct, Times.Exactly(1));
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ReturnsNull()
    {
        // Arrange
        var query = GetTenderByIdQueryBuilder.Default.WithId(1).Build();
        var ct = CancellationToken.None;
        SetupGetAllTendersAsync([]);

        // Act
        var result = await Handler.Handle(query, ct);

        // Assert
        result.Should().BeNull();
    }
}
