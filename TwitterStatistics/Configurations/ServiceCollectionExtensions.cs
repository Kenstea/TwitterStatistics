using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using TwitterStatistics.Service.Managers;
using TwitterStatistics.Service.Services;

namespace TwitterStatistics.Service.Configurations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<SampledTweetsPolling>();
            services.Configure<AppSettings>(configuration);
            services.AddHttpClient<ITwitterService, TwitterService>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicyForTooManyRequestsError())
                .AddPolicyHandler(GetRetryPolicyForHttpErrors());
            services.AddTransient<ITwitterStatisticsManager, TwitterStatisticsManager>();
            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicyForHttpErrors()
        {   
            // Start with a 5 second wait, doubling each attempt, up to 320 seconds with 7 attempts.
            TimeSpan RetryAttemptCalculation
                (int retryAttempt) => TimeSpan.FromSeconds(5 * Math.Pow(retryAttempt-1, 2));

            const int retryCount = 7;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(retryCount, RetryAttemptCalculation);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicyForTooManyRequestsError()
        {
            // Start with a 1 minute wait and double each attempt.
            // Note that every HTTP 429 received increases the time we must wait until rate limiting will no longer will be in effect for the account.
            TimeSpan RetryAttemptCalculation
                (int retryAttempt) => TimeSpan.FromSeconds(60 * Math.Pow(retryAttempt - 1, 2));

            // Set unlimited until rate limiting will no longer will be in effect for the account.
            const int retryCount = int.MaxValue;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(retryCount, RetryAttemptCalculation);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicyUnauthorizedRequest()
        {
            // Start with a 1 minute wait and double each attempt.
            // Note that every HTTP 429 received increases the time we must wait until rate limiting will no longer will be in effect for the account.
            TimeSpan RetryAttemptCalculation
                (int retryAttempt) => TimeSpan.FromSeconds(60 * Math.Pow(retryAttempt - 1, 2));

            // Set unlimited until rate limiting will no longer will be in effect for the account.
            const int retryCount = int.MaxValue;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(retryCount, RetryAttemptCalculation);
        }
    }
}
