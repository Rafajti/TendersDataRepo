using System.Net;
using System.Text;

namespace TendersData.IntegrationTests.Mocks;

public sealed class MockTendersGuruHandler : HttpMessageHandler
{
    private static readonly string Page1Json = """
        {
            "data": [
                {
                    "id": "1",
                    "date": "2024-06-15",
                    "title": "Integration Test Tender 1",
                    "description": "Description for tender 1",
                    "amount_eur": "15000.50",
                    "suppliers": [{"id": 1, "name": "Supplier A"}]
                },
                {
                    "id": "2",
                    "date": "2024-06-20",
                    "title": "Integration Test Tender 2",
                    "description": "Description for tender 2",
                    "amount_eur": "8500",
                    "suppliers": [{"id": 2, "name": "Supplier B"}, {"id": 3, "name": "Supplier C"}]
                },
                {
                    "id": "3",
                    "date": "2024-05-10",
                    "title": "Integration Test Tender 3",
                    "description": "Description for tender 3",
                    "amount_eur": "250000",
                    "suppliers": []
                }
            ]
        }
        """;

    private static readonly string EmptyPageJson = """{"data":[]}""";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? "";
        var query = request.RequestUri?.Query ?? "";

        if (!path.Contains("tenders", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        var page = 1;
        if (query.Contains("page=", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(query, @"page=(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var p))
            {
                page = p;
            }
        }

        var json = page == 1 ? Page1Json : EmptyPageJson;
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        return Task.FromResult(response);
    }
}
