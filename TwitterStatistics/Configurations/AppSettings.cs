namespace TwitterStatistics.Service.Configurations
{
    public class AppSettings
    {
        public TwitterApi TwitterApi { get; set; }
    }

    public class TwitterApi
    {
        public string BaseUrl { get; set; }
        public string ApiToken { get; set; }
        public string SampledStreamEndpoint { get; set; }
        public int PollingDurationInSeconds { get; set; }
        public int RequestTimeOutInSeconds { get; set; }
    }
}
