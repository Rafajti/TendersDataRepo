using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TendersData.Application.Tenders.Models;
using TendersData.Application.Tenders.Queries.GetTenderById;
using TendersData.Tests.Common.Builders;

namespace TendersData.Api.Tests.Controllers;

public class TendersControllerTests : TendersControllerMockHelper
{
    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResultWithTender()
    {
        // Arrange
        var id = 1;
        var expectedTender = TenderBuilder.Default
            .WithId(id)
            .WithTitle("Test Tender")
            .WithDescription("Test Description")
            .WithAmountEur(1000.50m)
            .WithSupplier(1, "Supplier 1")
            .Build();
        SetupGetById(id, expectedTender);

        // Act
        var result = await Controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedTender);
        okResult.StatusCode.Should().Be(200);
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsOkResultWithNull()
    {
        // Arrange
        var id = 999;
        SetupGetById(id, null);

        // Act
        var result = await Controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeNull();
        okResult.StatusCode.Should().Be(200);
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task GetById_WithDifferentIds_CallsMediatorWithCorrectQuery(int id)
    {
        // Arrange
        var expectedTender = TenderBuilder.Default
            .WithId(id)
            .WithTitle($"Tender {id}")
            .WithAmountEur(500m)
            .Build();
        SetupGetById(id, expectedTender);

        // Act
        var result = await Controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedTender);
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task GetById_WithZeroId_ThrowsValidationException()
    {
        // Arrange
        var id = 0;
        SetupGetByIdThrows(id, new FluentValidation.ValidationException("Validation failed"));

        // Act
        var act = () => Controller.GetById(id);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task GetById_WithNegativeId_ThrowsValidationException()
    {
        // Arrange
        var id = -1;
        SetupGetByIdThrows(id, new FluentValidation.ValidationException("Validation failed"));

        // Act
        var act = () => Controller.GetById(id);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }

    [Fact]
    public async Task GetById_WithTenderHavingMultipleSuppliers_ReturnsOkResultWithAllSuppliers()
    {
        // Arrange
        var id = 1;
        var expectedTender = TenderBuilder.Default
            .WithId(id)
            .WithTitle("Multi-Supplier Tender")
            .WithAmountEur(5000m)
            .WithSuppliers(
                SupplierBuilder.Default.WithId(1).WithName("Supplier 1").Build(),
                SupplierBuilder.Default.WithId(2).WithName("Supplier 2").Build(),
                SupplierBuilder.Default.WithId(3).WithName("Supplier 3").Build())
            .Build();
        SetupGetById(id, expectedTender);

        // Act
        var result = await Controller.GetById(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        var tender = okResult.Value as Tender;
        tender.Should().NotBeNull();
        tender!.Suppliers.Should().HaveCount(3);
        MediatorMock.Verify(
            m => m.Send(It.Is<GetTenderByIdQuery>(q => q.Id == id), It.IsAny<CancellationToken>()),
            Moq.Times.Once);
    }
}
