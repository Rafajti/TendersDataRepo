using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TendersData.IntegrationTests.Mocks;

namespace TendersData.IntegrationTests;

public class TendersDataWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TendersGuru:PagesCount"] = "1",
                ["TendersGuru:BaseUrl"] = "https://tenders.guru/api/pl/"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IHttpClientFactory, MockHttpClientFactory>();
        });
    }
}
