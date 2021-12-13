using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;
namespace FakeTwitterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleController : ControllerBase
    {
        private readonly ILogger<SampleController> _logger;
        private string JsonData { get; }

        public SampleController(ILogger<SampleController> logger)
        {
            _logger = logger;
            JsonData = System.IO.File.ReadAllText(@"[your file]");
        }

        [HttpGet]
        [Route("/2/tweets/sample/stream")]
        public async Task<string> Stream()
        {
            return await Task.FromResult(JsonData);
        }
    }
}
