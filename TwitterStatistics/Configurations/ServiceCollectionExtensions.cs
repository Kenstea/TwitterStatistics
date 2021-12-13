using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using TwitterStatistics.Service.Exceptions;
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
                .AddPolicyHandler(RetryPolicy.GetRetryPolicyForTooManyRequestsError())
                .AddPolicyHandler(RetryPolicy.GetRetryPolicyForHttpErrors());
            services.AddTransient<ITwitterStatisticsManager, TwitterStatisticsManager>();
            return services;
        }
    }
}
