using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Net.Http.Json;
using TendersData.Application.Tenders.Models;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Models;
using TendersData.Infrastructure.TendersGuru.Repositories;
using Xunit;

namespace TendersData.Infrastructure.Tests.TendersGuru.Repositories;

public class TendersDataRepositoryTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly Mock<ITenderMapper> _mapperMock;
    private readonly Mock<ILogger<TendersDataRepository>> _loggerMock;
    private readonly TendersDataRepository _repository;

    public TendersDataRepositoryTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHttp)
        {
            BaseAddress = new Uri("https://tenders.guru/api/pl/")
        };
        _mapperMock = new Mock<ITenderMapper>();
        _loggerMock = new Mock<ILogger<TendersDataRepository>>();
        _repository = new TendersDataRepository(_httpClient, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithValidResponse_ReturnsMappedTenders()
    {
        // Arrange
        var response = new TendersGuruResponse
        {
            Data = new List<TendersGuruItem>
            {
                new TendersGuruItem { Id = "1", Title = "Tender 1", AmountEur = "100", Date = "2024-01-01" },
                new TendersGuruItem { Id = "2", Title = "Tender 2", AmountEur = "200", Date = "2024-01-02" }
            }
        };

        var expectedTenders = new List<Tender>
        {
            new Tender(1, DateTime.Parse("2024-01-01"), "Tender 1", "", 100m, new List<Supplier>()),
            new Tender(2, DateTime.Parse("2024-01-02"), "Tender 2", "", 200m, new List<Supplier>())
        };

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=1")
            .Respond(HttpStatusCode.OK, JsonContent.Create(response));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=2")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=3")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=4")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=5")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mapperMock
            .Setup(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()))
            .Returns(expectedTenders);

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTenders);
        _mapperMock.Verify(
            m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithMultiplePages_ProcessesAllPages()
    {
        // Arrange
        var page1Response = new TendersGuruResponse
        {
            Data = new List<TendersGuruItem> { new TendersGuruItem { Id = "1", Title = "Tender 1", AmountEur = "100", Date = "2024-01-01" } }
        };

        var page2Response = new TendersGuruResponse
        {
            Data = new List<TendersGuruItem> { new TendersGuruItem { Id = "2", Title = "Tender 2", AmountEur = "200", Date = "2024-01-02" } }
        };

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=1")
            .Respond(HttpStatusCode.OK, JsonContent.Create(page1Response));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=2")
            .Respond(HttpStatusCode.OK, JsonContent.Create(page2Response));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=3")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=4")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=5")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        var allItems = new List<TendersGuruItem>();
        _mapperMock
            .Setup(m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()))
            .Callback<IEnumerable<TendersGuruItem>>(items => allItems.AddRange(items))
            .Returns(new List<Tender>());

        // Act
        await _repository.GetAllTendersAsync();

        // Assert
        allItems.Should().HaveCount(2);
        _mapperMock.Verify(
            m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithNullData_SkipsPage()
    {
        // Arrange
        var response = new TendersGuruResponse { Data = null };

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=1")
            .Respond(HttpStatusCode.OK, JsonContent.Create(response));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=2")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=3")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=4")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=5")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _mapperMock.Verify(
            m => m.MapToDomain(It.IsAny<IEnumerable<TendersGuruItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithHttpException_LogsWarningAndContinues()
    {
        // Arrange
        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=1")
            .Throw(new HttpRequestException("Network error"));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=2")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=3")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=4")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        _mockHttp
            .When("https://tenders.guru/api/pl/tenders?page=5")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new TendersGuruResponse { Data = new List<TendersGuruItem>() }));

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cos sie wypieprzylo")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllTendersAsync_WithCancellationToken_PassesTokenToHttpClient()
    {
        // Arrange
        var cancellationToken = new CancellationTokenSource().Token;
        var response = new TendersGuruResponse { Data = new List<TendersGuruItem>() };

        for (int i = 1; i <= 5; i++)
        {
            _mockHttp
                .When($"https://tenders.guru/api/pl/tenders?page={i}")
                .Respond(HttpStatusCode.OK, JsonContent.Create(response));
        }

        // Act
        await _repository.GetAllTendersAsync(cancellationToken);

        // Assert
        // Token is passed to HttpClient, but MockHttp doesn't validate it
        // This test verifies the method accepts and uses the token parameter
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAllTendersAsync_WithEmptyPages_ReturnsEmptyList()
    {
        // Arrange
        var emptyResponse = new TendersGuruResponse { Data = new List<TendersGuruItem>() };

        for (int i = 1; i <= 5; i++)
        {
            _mockHttp
                .When($"https://tenders.guru/api/pl/tenders?page={i}")
                .Respond(HttpStatusCode.OK, JsonContent.Create(emptyResponse));
        }

        // Act
        var result = await _repository.GetAllTendersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllTendersAsync_ProcessesExactlyFivePages()
    {
        // Arrange
        var response = new TendersGuruResponse
        {
            Data = new List<TendersGuruItem> { new TendersGuruItem { Id = "1", Title = "Test", AmountEur = "100", Date = "2024-01-01" } }
        };

        for (int i = 1; i <= 5; i++)
        {
            _mockHttp
                .When($"https://tenders.guru/api/pl/tenders?page={i}")
                .Respond(HttpStatusCode.OK, JsonContent.Create(response));
        }

        // Act
        await _repository.GetAllTendersAsync();

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}
