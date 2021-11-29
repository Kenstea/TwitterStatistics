namespace TwitterStatistics.Service.Models
{
    /// <summary>
    /// Represents statistics of sampled tweets.
    /// </summary>
    public class SampledTweetsStatistics
    {
        public int TotalSampledTweets { get; set; }
        public int AverageTweetsPerMinute { get; set; }
    }
}