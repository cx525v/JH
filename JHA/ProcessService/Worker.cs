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
            await _dataProcess.ProcessData();
            //Thread tid1 = new Thread(new ThreadStart(_dataProcess.ProcessData));
            //tid1.Start();

            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    await Task.Delay(3000, stoppingToken);

            //    _dataProcess.DisplayData();

            //}      
        }
   
      
    }
}