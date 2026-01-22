using Microsoft.Extensions.DependencyInjection;
using TendersData.Application.Tenders.Repositories;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Repositories;
using System.Net;
using Microsoft.Extensions.Configuration;


namespace TendersData.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITenderMapper, TenderMapper>();



        services.AddHttpClient<ITendersDataRepository, TendersDataRepository>(client =>
        {
            var baseUrl = configuration["TendersGuru:BaseUrl"];

            client.DefaultRequestHeaders.Add("Referer", "https://tenders.guru/api/pl");
            client.BaseAddress = new Uri(baseUrl ?? "https://tenders.guru/api/pl/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
