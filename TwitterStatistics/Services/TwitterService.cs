using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TwitterStatistics.Service.Configurations;
using TwitterStatistics.Service.Exceptions;
using TwitterStatistics.Service.Models;

namespace TwitterStatistics.Service.Services
{
    public class TwitterService : ITwitterService
    {
        private readonly HttpClient _client;
        private readonly IOptions<AppSettings> _settings;
        private readonly ILogger<TwitterService> _logger;

        public TwitterService(
            HttpClient client, 
            IOptions<AppSettings> settings, 
            ILogger<TwitterService> logger)
        {
            client.BaseAddress = new Uri(settings.Value.TwitterApi.BaseUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", settings.Value.TwitterApi.ApiToken);
            _client = client;
            _client.Timeout = TimeSpan.FromSeconds(settings.Value.TwitterApi.RequestTimeOutInSeconds);
            _settings = settings;
            _logger = logger;
        }

        public async Task SampleTweetStream(
            ConcurrentBag<SampledTweet> sampledTweets, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestUrl =
                    $"{_settings.Value.TwitterApi.BaseUrl}{_settings.Value.TwitterApi.SampledStreamEndpoint}";
                if (string.IsNullOrWhiteSpace(_settings.Value.TwitterApi.BaseUrl))
                {
                    throw new ArgumentNullException(nameof(_settings.Value.TwitterApi.BaseUrl),
                        "Base Uri is required.");
                }

                if(!Uri.TryCreate(requestUrl, UriKind.Absolute, out var requestUri))
                {
                    throw new InvalidUrlException("Request URL is invalid.", requestUrl);
                }

                if (string.IsNullOrWhiteSpace(_settings.Value.TwitterApi.SampledStreamEndpoint))
                {
                    throw new ArgumentNullException(nameof(_settings.Value.TwitterApi.SampledStreamEndpoint),
                        "Sampled Tweet Endpoint is required.");
                }

                _logger.LogInformation("Start request with URI: {uri}.", requestUrl);
                var sampleTweetStreamResponse = await _client.GetAsync(requestUri,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                
                sampleTweetStreamResponse.EnsureSuccessStatusCode();

                _logger.LogInformation("Requested successfully. Reading as string.");
                await using var sampleTweetStream =
                    await sampleTweetStreamResponse.Content.ReadAsStreamAsync(cancellationToken);
                using var streamReader = new StreamReader(sampleTweetStream);
                using var jsonReader = new JsonTextReader(streamReader)
                {
                    SupportMultipleContent = true
                };

                var jsonSerializer = new JsonSerializer();

                _logger.LogInformation("Started analyzing streaming...");
               
                //var tweetCount = 0;
                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    var loadedJson = await JToken.LoadAsync(jsonReader, cancellationToken);
                    if (!loadedJson.HasValues) throw new InvalidResponseException("Json is empty.");

                    var sampledTweet = loadedJson.ToObject<SampledTweet>(jsonSerializer);
                    if (sampledTweet is null) throw new InvalidResponseException("Object is null.");

                    sampledTweets.Add(sampledTweet);

                    //_logger.LogInformation("Tweet count: {0}\r\n", ++tweetCount);
                    //_logger.LogInformation("Tweet: {0}\r\n", JsonConvert.SerializeObject(sampledTweet,Formatting.Indented));
                }

                _logger.LogInformation(@"Streaming has stopped sending data for a reason but not an exception. 
Simply returns results and comes back if needed.");
            }
            catch (OperationCanceledException o) when (o.CancellationToken == cancellationToken)
            {
                _logger.LogInformation("GetSampledStream has been cancelled.");
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An unexpected error when calling Twitter API.");
                throw;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex,"GetSampledStream is timed out.");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Unable to get sampled stream from Twitter.");
                throw;
            }
        }
    }
}
