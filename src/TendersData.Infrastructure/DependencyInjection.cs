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
        var logger = serviceProvider.GetRequiredService<ILogger<ITendersDataRepository>>();
        
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(120),
            TimeoutStrategy.Pessimistic,
            onTimeoutAsync: (context, timespan, task) =>
            {
                logger.LogWarning("Request timeout po {Time}s", timespan.TotalSeconds);
                return Task.CompletedTask;
            });

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.GatewayTimeout ||
                            msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                            msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString() ?? outcome.Exception?.GetType().Name ?? "Unknown";
                    var requestUri = outcome.Result?.RequestMessage?.RequestUri?.ToString() ?? "Unknown";
                    
                    logger.LogWarning(
                        "Retry attempt {RetryCount}/3 for {RequestUri}. Status: {StatusCode}. Waiting {Delay}s before retry.",
                        retryAttempt,
                        requestUri,
                        statusCode,
                        timespan.TotalSeconds);
                });

        return Policy.WrapAsync(timeoutPolicy, retryPolicy);
    }
}
