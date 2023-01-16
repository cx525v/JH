using SampleService.Interfaces;

namespace SampleService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITweetClient _tweetClient;
   
        public Worker(ILogger<Worker> logger, ITweetClient client)
        {
            _logger = logger;
            _tweetClient = client;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
                await _tweetClient.GetTweetData(stoppingToken);
            }

        }
    }
}