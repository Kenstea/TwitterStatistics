using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using TwitterStatistics.Service.Configurations;
using TwitterStatistics.Service.Models;
using TwitterStatistics.Service.Services;
using Xunit;

namespace TwitterStatistics.UnitTest.Services
{
    public class TwitterServiceTests
    {
        private const string SampledStreamUrl = "https://api.twitter.com/2/tweets/sample/stream";
        private readonly MockRepository _mockRepository;
        private readonly MockHttpMessageHandler _mockHttp;
        private readonly Mock<ILogger<TwitterService>> _mockLogger;
        private readonly Mock<IOptions<AppSettings>> _mockOptions;

        /// <summary>
        /// Hard coded JSON of Sampled stream. 
        /// </summary>
        private string JsonData { get; }

        public TwitterServiceTests()
        {
            _mockOptions = new Mock<IOptions<AppSettings>>();
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mockHttp = new MockHttpMessageHandler();
            _mockLogger = new Mock<ILogger<TwitterService>>();

            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            if (directory == null) throw new ArgumentNullException("Cannot find JsonData");
            var path = Path.Combine(directory, "response_play.json");
            JsonData = File.ReadAllText(path);
        }

        private TwitterService CreateService()
        {
            return new TwitterService(_mockHttp.ToHttpClient(), _mockOptions.Object, _mockLogger.Object);
        }

        [Fact]
        [Trait("Category","HappyPath")]
        public async Task GetSampledStream_ValidSampledStreamUrl_ShouldReturnSampledTweets()
        {
            // Arrange            
            _mockHttp.When(SampledStreamUrl).Respond("application/json", JsonData);

            _mockOptions.Setup(e => e.Value).Returns(new AppSettings
            {
                TwitterApi = new TwitterApi
                {
                    ApiToken = "fakeToken",
                    BaseUrl = "https://api.twitter.com",
                    PollingDurationInSeconds = 100,
                    RequestTimeOutInSeconds = 100,
                    SampledStreamEndpoint = "/2/tweets/sample/stream"
                }
            });

            var service = this.CreateService();
            var bag = new ConcurrentBag<SampledTweet>();

            // Act
            await service.SampleTweetStream(bag, CancellationToken.None);

            // Assert

            Assert.NotNull(bag);
            Assert.True(bag.Count > 0);
            this._mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetSampledStream_WithCancellationToken_ShouldThrowOperationCanceledException()
        {
            // Arrange            
            _mockHttp.When(string.Empty).Throw(new ArgumentNullException());

            _mockOptions.Setup(e => e.Value).Returns(new AppSettings
            {
                TwitterApi = new TwitterApi
                {
                    ApiToken = "fakeToken",
                    BaseUrl = "https://api.twitter.com",
                    PollingDurationInSeconds = 100,
                    RequestTimeOutInSeconds = 100,
                    SampledStreamEndpoint = "/2/tweets/sample/stream"
                }
            });

            var service = this.CreateService();
            var bag = new ConcurrentBag<SampledTweet>();
            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            // Act/Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                service.SampleTweetStream(bag, cancellationSource.Token));
            this._mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetSampledStream_ApiThrowHttpRequestException_ShouldThrowHttpRequestException()
        {
            // Arrange            
            var exception = new HttpRequestException();
            _mockHttp.When(SampledStreamUrl).Throw(exception);

            _mockOptions.Setup(e => e.Value).Returns(new AppSettings
            {
                TwitterApi = new TwitterApi
                {
                    ApiToken = "fakeToken",
                    BaseUrl = "https://api.twitter.com",
                    PollingDurationInSeconds = 100,
                    RequestTimeOutInSeconds = 100,
                    SampledStreamEndpoint = "/2/tweets/sample/stream"
                }
            });

            var service = this.CreateService();
            var bag = new ConcurrentBag<SampledTweet>();

            // Act
            // Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () => await service.SampleTweetStream(bag));
            this._mockRepository.VerifyAll();
        }

        [Fact]
        public async Task GetSampledStream_EmptySampledStreamUrl_ShouldThrowArgumentNullException()
        {
            // Arrange            
            _mockHttp.When(string.Empty).Throw(new ArgumentNullException());

            _mockOptions.Setup(e => e.Value).Returns(new AppSettings
            {
                TwitterApi = new TwitterApi
                {
                    ApiToken = "fakeToken",
                    BaseUrl = "https://api.twitter.com",
                    PollingDurationInSeconds = 100,
                    RequestTimeOutInSeconds = 100,
                    SampledStreamEndpoint = string.Empty
                }
            });

            var service = this.CreateService();

            var bag = new ConcurrentBag<SampledTweet>();
            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.SampleTweetStream(bag, CancellationToken.None));
            this._mockRepository.VerifyAll();
        }


        [Fact]
        public async Task GetSampledStream_ApiReturnEmptyContent_ShouldReturnSilently()
        {
            // Arrange            
            _mockHttp.When(SampledStreamUrl).Respond("application/json", string.Empty);

            _mockOptions.Setup(e => e.Value).Returns(new AppSettings
            {
                TwitterApi = new TwitterApi
                {
                    ApiToken = "fakeToken",
                    BaseUrl = "https://api.twitter.com",
                    PollingDurationInSeconds = 100,
                    RequestTimeOutInSeconds = 100,
                    SampledStreamEndpoint = "/2/tweets/sample/stream"
                }
            });

            var service = this.CreateService();
            var bag = new ConcurrentBag<SampledTweet>();

            // Act
            // Assert
            await service.SampleTweetStream(bag);
            Assert.Empty(bag);
            this._mockRepository.VerifyAll();
        }
    }
}
