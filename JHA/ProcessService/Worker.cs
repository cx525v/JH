using ProcessService.Interfaces;

namespace ProcessService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDataProcessService _dataProcess;
        public Worker(ILogger<Worker> logger, IDataProcessService dataProcess)
        {
            _logger = logger;
            _dataProcess = dataProcess;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {        
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
                 _dataProcess.ProcessData();
            }
        }
      
    }
}