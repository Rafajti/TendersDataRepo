using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TendersData.Api.Controllers;
using Xunit;

namespace TendersData.Api.Tests.Controllers;

public class TendersControllerTests
{
    private readonly TendersController _controller;

    public TendersControllerTests()
    {
        _controller = new TendersController();
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
        okResult.StatusCode.Should().Be(200);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetById_WithDifferentIds_ReturnsOkResult(int id)
    {
        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(true);
    }

    [Fact]
    public async Task GetById_WithZeroId_ReturnsOkResult()
    {
        // Arrange
        var id = 0;

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_WithNegativeId_ReturnsOkResult()
    {
        // Arrange
        var id = -1;

        // Act
        var result = await _controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }
}
