using System;
using System.Threading;
using System.Threading.Tasks;
using TwitterStatistics.Service.Models;

namespace TwitterStatistics.Service.Managers
{
    public class TwitterStatisticsManager : ITwitterStatisticsManager
    {
        public async Task<SampledTweetsStatistics> GetSampledTweetsStatistics()
        {
            const double preventZeroDivisionNumber = 0.0001;
            const double minuteToMilliseconds = 60000.0;

            var tweetStats = new SampledTweetsStatistics
            {
                TotalSampledTweets = SampledTweetsPolling.SampledTweetBag.Count
            };

            var elapsedMilliseconds = SampledTweetsPolling.ElapsedMilliseconds != 0
                ? SampledTweetsPolling.ElapsedMilliseconds
                : preventZeroDivisionNumber;

            tweetStats.AverageTweetsPerMinute =
                (int)Math.Round((SampledTweetsPolling.SampledTweetBag.Count / elapsedMilliseconds) * minuteToMilliseconds, 0);

            await Task.CompletedTask;
            return tweetStats;
        }
    }
}
