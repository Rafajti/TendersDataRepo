using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TendersData.Api.Controllers;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Queries.GetTenderById;

namespace TendersData.Api.Tests.Controllers;

public class TendersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly TendersController _controller;

    public TendersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new TendersController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResultWithTender()
    {
        // Arrange
        var id = 1;
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

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTender);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedTender);
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsOkResultWithNull()
    {
        // Arrange
        var id = 999;
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tender?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeNull();
        okResult.StatusCode.Should().Be(200);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetById_WithDifferentIds_CallsMediatorWithCorrectQuery(int id)
    {
        // Arrange
        var expectedTender = new Tender(
            Id: id,
            Date: DateTime.UtcNow,
            Title: $"Tender {id}",
            Description: "Description",
            AmountEur: 500m,
            Suppliers: new List<Supplier>()
        );

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTender);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedTender);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithZeroId_ReturnsOkResult()
    {
        // Arrange
        var id = 0;
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tender?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithNegativeId_ReturnsOkResult()
    {
        // Arrange
        var id = -1;
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tender?)null);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetById_WithTenderHavingMultipleSuppliers_ReturnsOkResultWithAllSuppliers()
    {
        // Arrange
        var id = 1;
        var expectedTender = new Tender(
            Id: id,
            Date: DateTime.UtcNow,
            Title: "Multi-Supplier Tender",
            Description: "Description",
            AmountEur: 5000m,
            Suppliers: new List<Supplier>
            {
                new Supplier(1, "Supplier 1"),
                new Supplier(2, "Supplier 2"),
                new Supplier(3, "Supplier 3")
            }
        );

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTender);

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var tender = okResult!.Value as Tender;
        tender.Should().NotBeNull();
        tender!.Suppliers.Should().HaveCount(3);

        _mediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
