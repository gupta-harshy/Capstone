using Microsoft.Extensions.Logging;
using Temporalio.Worker;

namespace CheckoutOrchestrator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly TemporalWorker _temporalWorker;

        public Worker(ILogger<Worker> logger, TemporalWorker temporalWorker)
        {
            _logger = logger;
            _temporalWorker = temporalWorker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker starting at: {time}", DateTimeOffset.Now);
            await _temporalWorker.ExecuteAsync(stoppingToken);
            _logger.LogInformation("Temporal Worker stopped at: {time}", DateTimeOffset.Now);
        }
    }
}
