using FluentAssertions;
using Moq;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Queries.GetTenderById;
using TendersData.Application.Tenders.Repositories;

namespace TendersData.Application.Tests.Tenders.Queries.GetTenderById;

public class GetTenderByIdQueryHandlerTests
{
    private readonly Mock<ITendersDataRepository> _repositoryMock;
    private readonly GetTenderByIdQueryHandler _handler;

    public GetTenderByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ITendersDataRepository>();
        _handler = new GetTenderByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsTender()
    {
        // Arrange
        var id = 1;
        var query = new GetTenderByIdQuery(id);
        var cancellationToken = CancellationToken.None;
        var expectedTender = new Tender(
            Id: id,
            Date: DateTime.UtcNow,
            Title: "Test Tender",
            Description: "Test Description",
            AmountEur: 1000.50m,
            Suppliers: new List<Supplier>
            {
                new Supplier(1, "Supplier 1")
            }
        );

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tender> { expectedTender });

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedTender);
        result!.Id.Should().Be(id);

        _repositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var id = 999;
        var query = new GetTenderByIdQuery(id);
        var cancellationToken = CancellationToken.None;

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tender>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleTenders_ReturnsCorrectTender()
    {
        // Arrange
        var id = 2;
        var query = new GetTenderByIdQuery(id);
        var cancellationToken = CancellationToken.None;
        var tenders = new List<Tender>
        {
            new Tender(1, DateTime.UtcNow, "Tender 1", "Desc 1", 100m, new List<Supplier>()),
            new Tender(2, DateTime.UtcNow, "Tender 2", "Desc 2", 200m, new List<Supplier>()),
            new Tender(3, DateTime.UtcNow, "Tender 3", "Desc 3", 300m, new List<Supplier>())
        };

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenders);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Title.Should().Be("Tender 2");

        _repositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task Handle_WithDifferentIds_CallsRepository(int id)
    {
        // Arrange
        var query = new GetTenderByIdQuery(id);
        var cancellationToken = CancellationToken.None;
        var tenders = new List<Tender>
        {
            new Tender(id, DateTime.UtcNow, $"Tender {id}", "Description", 500m, new List<Supplier>())
        };

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenders);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);

        _repositoryMock.Verify(
            r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesItToRepository()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = new CancellationTokenSource().Token;

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(cancellationToken))
            .ReturnsAsync(new List<Tender>());

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.GetAllTendersAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyRepository_ReturnsNull()
    {
        // Arrange
        var query = new GetTenderByIdQuery(1);
        var cancellationToken = CancellationToken.None;

        _repositoryMock
            .Setup(r => r.GetAllTendersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tender>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().BeNull();
    }
}
