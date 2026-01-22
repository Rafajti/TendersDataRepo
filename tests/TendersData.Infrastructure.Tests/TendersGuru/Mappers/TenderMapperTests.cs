using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Tests.Common.Builders;

namespace TendersData.Infrastructure.Tests.TendersGuru.Mappers;

public class TenderMapperTests : TenderMapperMockHelper
{
    [Fact]
    public void MapToDomain_WithValidItem_ReturnsTender()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithDate("2024-01-15")
            .WithTitle("Test Tender")
            .WithDescription("Test Description")
            .WithAmountEur("1000.50")
            .WithSuppliers(SupplierGuruBuilder.Default.WithId(1).WithName("Supplier 1").Build())
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Test Tender");
        result.Description.Should().Be("Test Description");
        result.AmountEur.Should().Be(1000.50m);
        result.Date.Should().Be(DateTime.Parse("2024-01-15"));
        result.Suppliers.Should().HaveCount(1);
        result.Suppliers[0].Id.Should().Be(1);
        result.Suppliers[0].Name.Should().Be("Supplier 1");
    }

    [Fact]
    public void MapToDomain_WithMultipleItems_ReturnsMultipleTenders()
    {
        // Arrange
        var items = new[]
        {
            TendersGuruItemBuilder.Default.WithId("1").WithTitle("Tender 1").WithAmountEur("100").WithDate("2024-01-01").Build(),
            TendersGuruItemBuilder.Default.WithId("2").WithTitle("Tender 2").WithAmountEur("200").WithDate("2024-01-02").Build(),
            TendersGuruItemBuilder.Default.WithId("3").WithTitle("Tender 3").WithAmountEur("300").WithDate("2024-01-03").Build()
        };

        // Act
        var result = Mapper.MapToDomain(items).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[1].Id.Should().Be(2);
        result[2].Id.Should().Be(3);
    }

    [Fact]
    public void MapToDomain_WithInvalidId_LogsWarningAndUsesZero()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("invalid")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Id.Should().Be(0);
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Incorrect ID tender")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void MapToDomain_WithInvalidAmount_ReturnsZero()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("invalid")
            .WithDate("2024-01-01")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.AmountEur.Should().Be(0);
    }

    [Fact]
    public void MapToDomain_WithInvalidDate_ReturnsMinValue()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("invalid-date")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Date.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void MapToDomain_WithNullSuppliers_ReturnsEmptyList()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .WithSuppliers()
            .Build();
        item.Suppliers = null;

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Fact]
    public void MapToDomain_WithEmptySuppliers_ReturnsEmptyList()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .WithSuppliers()
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Fact]
    public void MapToDomain_WithMultipleSuppliers_ReturnsAllSuppliers()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .WithSuppliers(
                SupplierGuruBuilder.Default.WithId(1).WithName("Supplier 1").Build(),
                SupplierGuruBuilder.Default.WithId(2).WithName("Supplier 2").Build(),
                SupplierGuruBuilder.Default.WithId(3).WithName("Supplier 3").Build())
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers.Should().HaveCount(3);
        result.Suppliers[0].Name.Should().Be("Supplier 1");
        result.Suppliers[1].Name.Should().Be("Supplier 2");
        result.Suppliers[2].Name.Should().Be("Supplier 3");
    }

    [Fact]
    public void MapToDomain_WithNullSupplierName_UsesEmptyString()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .WithSuppliers(SupplierGuruBuilder.Default.WithId(1).WithName(null).Build())
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers[0].Name.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithNullTitle_UsesEmptyString()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle(null!)
            .WithDescription("Test")
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Title.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithNullDescription_UsesEmptyString()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithDescription(null!)
            .WithAmountEur("100")
            .WithDate("2024-01-01")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithDecimalAmount_ParsesCorrectly()
    {
        // Arrange
        var item = TendersGuruItemBuilder.Default
            .WithId("1")
            .WithTitle("Test")
            .WithAmountEur("1234.56")
            .WithDate("2024-01-01")
            .Build();

        // Act
        var result = Mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.AmountEur.Should().Be(1234.56m);
    }

    [Fact]
    public void MapToDomain_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var items = Enumerable.Empty<TendersData.Infrastructure.TendersGuru.Models.TendersGuruItem>();

        // Act
        var result = Mapper.MapToDomain(items);

        // Assert
        result.Should().BeEmpty();
    }
}
