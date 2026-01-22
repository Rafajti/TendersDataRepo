using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TendersData.Application.Tenders.Models;
using static TendersData.Application.Tenders.TendersConstants;

namespace TendersData.IntegrationTests;

[Trait("Category", "Integration")]
public class TendersApiIntegrationTests : IClassFixture<TendersDataWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TendersApiIntegrationTests(TendersDataWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        EnsureCacheReadyAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureCacheReadyAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            var response = await _client.GetAsync("/api/tenders?PageNumber=1&PageSize=1");
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
                if (body is { TotalCount: > 0 })
                    return;
            }
            await Task.Delay(300);
        }
        throw new InvalidOperationException("Cache was not populated within 15 seconds.");
    }

    [Fact]
    public async Task GET_tenders_returns_200_and_list()
    {
        // Arrange
        var requestUri = "/api/tenders";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        body.TotalCount.Should().BeGreaterThan(0);
        body.PageNumber.Should().Be(1);
        body.PageSize.Should().Be(DefaultPageSize);
    }

    [Fact]
    public async Task GET_tenders_with_pagination_returns_200_and_paged_result()
    {
        // Arrange
        var requestUri = "/api/tenders?PageNumber=1&PageSize=10";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        body!.PageNumber.Should().Be(1);
        body.PageSize.Should().Be(10);
        body.Data.Should().NotBeNull();
        body.TotalCount.Should().BeGreaterThan(0);
        body.TotalPages.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GET_tenders_by_id_existing_returns_200_and_tender()
    {
        // Arrange
        var tenderId = 1;
        var requestUri = $"/api/tenders/{tenderId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tender = await response.Content.ReadFromJsonAsync<Tender>();
        tender.Should().NotBeNull();
        tender!.Id.Should().Be(tenderId);
        tender.Title.Should().Contain("Integration Test Tender 1");
        tender.AmountEur.Should().Be(15000.50m);
        tender.Suppliers.Should().HaveCount(1);
    }

    [Fact]
    public async Task GET_tenders_by_id_non_existent_returns_200_or_204_with_null_body()
    {
        // Arrange
        var nonExistentId = 99999;
        var requestUri = $"/api/tenders/{nonExistentId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent).Should().BeTrue();
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Trim().Should().Be("null");
        }
    }

    [Fact]
    public async Task GET_tenders_by_id_zero_returns_400_validation()
    {
        // Arrange
        var invalidId = 0;
        var requestUri = $"/api/tenders/{invalidId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ProblemDetailsJsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Title.Should().Contain("Validation");
        if (problem.Extensions is not null)
            problem.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task GET_tenders_by_id_negative_returns_400_validation()
    {
        // Arrange
        var invalidId = -1;
        var requestUri = $"/api/tenders/{invalidId}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(ProblemDetailsJsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
    }

    [Fact]
    public async Task GET_tenders_with_filters_MinPrice_MaxPrice_returns_filtered()
    {
        // Arrange
        var minPrice = 1000m;
        var maxPrice = 20000m;
        var requestUri = $"/api/tenders?PageNumber=1&PageSize=10&MinPriceEur={minPrice}&MaxPriceEur={maxPrice}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        foreach (var t in body.Data!)
        {
            t.AmountEur.Should().BeGreaterThanOrEqualTo(minPrice);
            t.AmountEur.Should().BeLessThanOrEqualTo(maxPrice);
        }
    }

    [Fact]
    public async Task GET_tenders_with_filters_DateFrom_DateTo_returns_filtered()
    {
        // Arrange
        var dateFrom = new DateTime(2024, 5, 1);
        var dateTo = new DateTime(2024, 6, 30);
        var requestUri = $"/api/tenders?PageNumber=1&PageSize=10&DateFrom={dateFrom:yyyy-MM-dd}&DateTo={dateTo:yyyy-MM-dd}";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        body!.Data.Should().NotBeNull();
        foreach (var t in body.Data!)
        {
            t.Date.Should().BeOnOrAfter(dateFrom);
            t.Date.Should().BeOnOrBefore(dateTo);
        }
    }

    [Fact]
    public async Task GET_tenders_with_sort_by_price_asc_returns_ordered()
    {
        // Arrange
        var requestUri = "/api/tenders?PageNumber=1&PageSize=10&SortBy=price&SortOrder=asc";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        var list = body!.Data!.ToList();
        if (list.Count >= 2)
            list[0].AmountEur.Should().BeLessThanOrEqualTo(list[1].AmountEur);
    }

    [Fact]
    public async Task GET_tenders_with_sort_by_date_desc_returns_ordered()
    {
        // Arrange
        var requestUri = "/api/tenders?PageNumber=1&PageSize=10&SortBy=date&SortOrder=desc";

        // Act
        var response = await _client.GetAsync(requestUri);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<Tender>>();
        body.Should().NotBeNull();
        var list = body!.Data!.ToList();
        if (list.Count >= 2)
            list[0].Date.Should().BeOnOrAfter(list[1].Date);
    }

    private static readonly JsonSerializerOptions ProblemDetailsJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class ProblemDetailsDto
    {
        public int Status { get; set; }
        public string? Title { get; set; }
        public Dictionary<string, JsonElement>? Extensions { get; set; }
    }
}
