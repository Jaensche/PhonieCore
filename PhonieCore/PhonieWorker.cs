using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PhonieCore
{
    public class PhonieWorker : BackgroundService
    {
        private readonly ILogger<PhonieWorker> _logger;

        public PhonieWorker(ILogger<PhonieWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Worker running at: {DateTime.Now}");
            await Task.Factory.StartNew(x => { new Radio(); }, new object(), stoppingToken);
        }
    }
}
