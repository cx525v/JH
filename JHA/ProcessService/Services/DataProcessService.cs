using Newtonsoft.Json;
using ProcessService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;

namespace ProcessService.Services
{
    public class DataProcessService: IDataProcessService
    {
        private readonly ILogger<DataProcessService> _logger;
        private readonly IConsumerBuilderHandler _consumerHandler;
        private readonly IUpdateDataService _updateDataService;
        public DataProcessService(ILogger<DataProcessService> logger, IConsumerBuilderHandler consumerHandler, IUpdateDataService updateDataService)
        {
            _logger = logger;
            _consumerHandler = consumerHandler;
            _consumerHandler.Topic = AppConstants.SAMPLE_TOPIC;
            _consumerHandler.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _consumerHandler.GroupId = AppConstants.SAMPLE_GROUP_ID;
            _consumerHandler.ProcessCompleted += _consumerHandler_ProcessCompleted;
            _updateDataService = updateDataService;
        }

        private void _consumerHandler_ProcessCompleted(string data)
        {
            _logger.LogInformation($"get data completed: {data}");

            List<TwitterRecord> BulkRecords = JsonConvert.DeserializeObject<List<TwitterRecord>>(data);

            _updateDataService.UpdateData(BulkRecords).ConfigureAwait(false) ;
        }

        //get data from producer with the topic
        public void ProcessData()
        {
            _consumerHandler.Subscribe();       
        }

       
    }
}
