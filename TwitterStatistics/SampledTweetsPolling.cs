using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitterStatistics.Service.Models;
using TwitterStatistics.Service.Services;

namespace TwitterStatistics.Service
{
    /// <summary>
    /// Poll and store data in memory in realtime from Twitter stream.
    /// </summary>
    internal sealed class SampledTweetsPolling:BackgroundService
    {
        public static readonly ConcurrentBag<SampledTweet> SampledTweetBag = new();
        public static long ElapsedMilliseconds => StopWatch.ElapsedMilliseconds;

        private readonly ITwitterService _twitterService;
        private readonly ILogger<SampledTweetsPolling> _logger;
        private static readonly Stopwatch StopWatch = new();

        public SampledTweetsPolling(
            ITwitterService twitterService, 
            ILogger<SampledTweetsPolling> logger)
        {
            _twitterService = twitterService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    StopWatch.Start();
                    _logger.LogInformation("Start polling.");
                    await _twitterService.SampleTweetStream(SampledTweetBag, stoppingToken);
                    _logger.LogInformation("Polling successfully.");
                }
                catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    StopWatch.Stop();
                    _logger.LogError(e, "The account is unauthorized. Stop polling.");
                    throw;
                }
                catch (Exception e)
                {
                    StopWatch.Stop();
                    _logger.LogError(e,"Unexpected error occurred when get sampled stream.");
                    //ignore
                }

                StopWatch.Stop();
            }
        }
    }
}
