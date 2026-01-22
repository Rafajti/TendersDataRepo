using FluentAssertions;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Queries.GetTenderById;
using Xunit;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public class GetTenderByIdQueryHandlerTests
{
    private readonly GetTenderByIdQueryHandler _handler;

    public GetTenderByIdQueryHandlerTests()
    {
        _handler = new GetTenderByIdQueryHandler();
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsTender()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Tender>();
    }

    [Fact]
    public async Task Handle_WithAnyId_ReturnsTenderWithExpectedProperties()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Title.Should().Be("Tittle");
        result.Description.Should().Be("Desc");
        result.AmountEur.Should().Be(10);
        result.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithDifferentIds_ReturnsTender(int id)
    {
        // Arrange
        var query = new GetTenderByIdQuery(id);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Tender>();
    }

    [Fact]
    public async Task Handle_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ReturnsTenderWithEmptySuppliersList()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsTenderWithDate()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(new DateTime()); 
    }
}
