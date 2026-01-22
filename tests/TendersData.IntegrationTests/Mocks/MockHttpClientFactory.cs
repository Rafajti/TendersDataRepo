using System.Net.Http;

namespace TendersData.IntegrationTests.Mocks;

public sealed class MockHttpClientFactory : IHttpClientFactory
{
    private static readonly Uri DefaultBaseAddress = new("https://tenders.guru/api/pl/");
    private readonly HttpMessageHandler _handler;

    public MockHttpClientFactory()
    {
        _handler = new MockTendersGuruHandler();
    }

    public HttpClient CreateClient(string name)
    {
        return new HttpClient(_handler)
        {
            BaseAddress = DefaultBaseAddress
        };
    }
}
