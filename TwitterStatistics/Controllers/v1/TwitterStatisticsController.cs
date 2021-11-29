using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TwitterStatistics.Service.Managers;
using TwitterStatistics.Service.Models;

namespace TwitterStatistics.Service.Controllers.v1
{
    [Route("[controller]")]
    [ApiController]
    public class TwitterStatisticsController : ControllerBase
    {
        private readonly ITwitterStatisticsManager _tweetComputationManager;

        public TwitterStatisticsController(ITwitterStatisticsManager tweetComputationManager)
        {
            _tweetComputationManager = tweetComputationManager;
        }     

        [HttpGet]
        public async Task<SampledTweetsStatistics> SampleTweetStatistics(CancellationToken cancellationToken = default)
        {
            var tweets = await _tweetComputationManager.GetSampledTweetsStatistics(cancellationToken);

            return tweets;
        }
    }
}
