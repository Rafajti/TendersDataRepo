using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;
using Xunit;

namespace TendersData.Infrastructure.Tests.TendersGuru.Mappers;

public class TenderMapperTests
{
    private readonly Mock<ILogger<TenderMapper>> _loggerMock;
    private readonly TenderMapper _mapper;

    public TenderMapperTests()
    {
        _loggerMock = new Mock<ILogger<TenderMapper>>();
        _mapper = new TenderMapper(_loggerMock.Object);
    }

    [Fact]
    public void MapToDomain_WithValidItem_ReturnsTender()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Date = "2024-01-15",
            Title = "Test Tender",
            Description = "Test Description",
            AmountEur = "1000.50",
            Suppliers = new List<SupplierGuru>
            {
                new SupplierGuru { Id = 1, Name = "Supplier 1" }
            }
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

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
        var items = new List<TendersGuruItem>
        {
            new TendersGuruItem { Id = "1", Title = "Tender 1", AmountEur = "100", Date = "2024-01-01" },
            new TendersGuruItem { Id = "2", Title = "Tender 2", AmountEur = "200", Date = "2024-01-02" },
            new TendersGuruItem { Id = "3", Title = "Tender 3", AmountEur = "300", Date = "2024-01-03" }
        };

        // Act
        var result = _mapper.MapToDomain(items).ToList();

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
        var item = new TendersGuruItem
        {
            Id = "invalid",
            Title = "Test",
            AmountEur = "100",
            Date = "2024-01-01"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Id.Should().Be(0);
        _loggerMock.Verify(
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
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "invalid",
            Date = "2024-01-01"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.AmountEur.Should().Be(0);
    }

    [Fact]
    public void MapToDomain_WithInvalidDate_ReturnsMinValue()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "100",
            Date = "invalid-date"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Date.Should().Be(DateTime.MinValue);
    }

    [Fact]
    public void MapToDomain_WithNullSuppliers_ReturnsEmptyList()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "100",
            Date = "2024-01-01",
            Suppliers = null
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Fact]
    public void MapToDomain_WithEmptySuppliers_ReturnsEmptyList()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "100",
            Date = "2024-01-01",
            Suppliers = new List<SupplierGuru>()
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers.Should().NotBeNull();
        result.Suppliers.Should().BeEmpty();
    }

    [Fact]
    public void MapToDomain_WithMultipleSuppliers_ReturnsAllSuppliers()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "100",
            Date = "2024-01-01",
            Suppliers = new List<SupplierGuru>
            {
                new SupplierGuru { Id = 1, Name = "Supplier 1" },
                new SupplierGuru { Id = 2, Name = "Supplier 2" },
                new SupplierGuru { Id = 3, Name = "Supplier 3" }
            }
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

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
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "100",
            Date = "2024-01-01",
            Suppliers = new List<SupplierGuru>
            {
                new SupplierGuru { Id = 1, Name = null }
            }
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Suppliers[0].Name.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithNullTitle_UsesEmptyString()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = null,
            Description = "Test",
            AmountEur = "100",
            Date = "2024-01-01"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Title.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithNullDescription_UsesEmptyString()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            Description = null,
            AmountEur = "100",
            Date = "2024-01-01"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void MapToDomain_WithDecimalAmount_ParsesCorrectly()
    {
        // Arrange
        var item = new TendersGuruItem
        {
            Id = "1",
            Title = "Test",
            AmountEur = "1234.56",
            Date = "2024-01-01"
        };

        // Act
        var result = _mapper.MapToDomain(new[] { item }).First();

        // Assert
        result.AmountEur.Should().Be(1234.56m);
    }

    [Fact]
    public void MapToDomain_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var items = Enumerable.Empty<TendersGuruItem>();

        // Act
        var result = _mapper.MapToDomain(items);

        // Assert
        result.Should().BeEmpty();
    }
}
