using System.Net;
using System.Text;

namespace TendersData.Infrastructure.Tests.TendersGuru.Services;

/// <summary>
internal sealed class FakeTendersGuruHttpHandler : HttpMessageHandler
{
    private static readonly string Page1Json = """
        {"data":[{"id":"1","date":"2024-06-15","title":"Unit Test Tender","description":"Desc","amount_eur":"5000","suppliers":[{"id":1,"name":"S1"}]}]}
        """;

    private static readonly string EmptyJson = """{"data":[]}""";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var query = request.RequestUri?.Query ?? "";
        var page = 1;
        if (query.Contains("page=", StringComparison.OrdinalIgnoreCase))
        {
            var m = System.Text.RegularExpressions.Regex.Match(query, @"page=(\d+)");
            if (m.Success && int.TryParse(m.Groups[1].Value, out var p))
                page = p;
        }

        var json = page == 1 ? Page1Json : EmptyJson;
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
