using System.Threading;
using System.Threading.Tasks;
using TwitterStatistics.Service.Models;

namespace TwitterStatistics.Service.Managers
{
    public interface ITwitterStatisticsManager
    {
        Task<SampledTweetsStatistics> GetSampledTweetsStatistics(CancellationToken cancellationToken);
    }
}