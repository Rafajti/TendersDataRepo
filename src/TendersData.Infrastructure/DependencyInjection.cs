using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using TendersData.Application.Tenders.Repositories;
using TendersData.Infrastructure.TendersGuru.Mappers;
using TendersData.Infrastructure.TendersGuru.Repositories;


namespace TendersData.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddScoped<ITenderMapper, TenderMapper>();
        services.AddTendersClient(configuration);

        return services;
    }

    public static IServiceCollection AddTendersClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<ITendersDataRepository, TendersDataRepository>(client =>
        {
            var baseUrl = configuration["TendersGuru:BaseUrl"];

            client.DefaultRequestHeaders.Add("Referer", "https://tenders.guru/api/pl");
            client.BaseAddress = new Uri(baseUrl ?? "https://tenders.guru/api/pl/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler((serviceProvider, request) => GetRetryPolicy(serviceProvider));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<ITendersDataRepository>>();

                    logger.LogWarning("Retry {Attempt} po {Time}s z powodu: {Reason}",
                        retryAttempt,
                        timespan.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString());
                });
    }
}
