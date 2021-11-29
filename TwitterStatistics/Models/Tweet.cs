using Newtonsoft.Json;

namespace TwitterStatistics.Service.Models
{
    /// <summary>
    /// Represent a block of tweet info sampled from stream.
    /// </summary>
    public class Tweet
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}