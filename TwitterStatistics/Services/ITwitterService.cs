using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using TwitterStatistics.Service.Models;

namespace TwitterStatistics.Service.Services
{
    public interface ITwitterService
    {
        /// <summary>
        /// Samples tweet stream from Twitter which represents 1% of total at the time being.
        /// </summary>
        /// <param name="sampledTweets">A collection to store data from this stream and feed to upper stream.</param>
        /// <param name="cancellationToken">Allow to cancel streaming.</param>
        /// <returns>This method will not return anything since it will keep streaming data continuously from Twitter API.</returns>
        Task SampleTweetStream(
            ConcurrentBag<SampledTweet> sampledTweets,
            CancellationToken cancellationToken = default);
    }
}
