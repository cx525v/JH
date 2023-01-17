using Newtonsoft.Json;
using ProcessService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;

namespace ProcessService.Services
{
    public class DataProcessService: IDataProcessService
    {
      
        readonly TweetResponse tweetResponse = new TweetResponse();

        private readonly ILogger<DataProcessService> _logger;
        private readonly IConsumerBuilderHandler _consumerHandler;
        private readonly IProducerBuilderHandler _producerHandler;
        public DataProcessService(ILogger<DataProcessService> logger, IConsumerBuilderHandler consumerHandler, IProducerBuilderHandler producerHandler)
        {
            _logger = logger;
            _consumerHandler = consumerHandler;
            _consumerHandler.Topic = AppConstants.SAMPLE_TOPIC;
            _consumerHandler.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _consumerHandler.GroupId = "test-consumer-group";
            _consumerHandler.ProcessCompleted += _consumerHandler_ProcessCompleted;
            
            _producerHandler = producerHandler;
            _producerHandler.Topic = AppConstants.PROCESS_TOPIC;
            _producerHandler.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _producerHandler.ProcessCompleted += _producerHandler_ProcessCompleted;
        }

        private void _producerHandler_ProcessCompleted(string data)
        {
            _logger.LogInformation($"publish completed: {data}");
        }

        private void _consumerHandler_ProcessCompleted(string data)
        {
            _logger.LogInformation($"get data completed: {data}");

            List<TwitterRecord> BulkRecords = JsonConvert.DeserializeObject<List<TwitterRecord>>(data);

            UpdateData(BulkRecords).ConfigureAwait(false) ;
        }

        //get data from producer with the topic
        public void ProcessData()
        {
            _consumerHandler.Subscribe();       
        }

       //data can be saved into database, here upsert into memory
        private async Task UpdateData(List<TwitterRecord> records)
        {

            IDictionary<string, int> hashDict = new Dictionary<string, int>();

            records.ForEach((record) =>
            {
                record.Data.Entities.Hashtags.ForEach(tag =>
                {
                    if (tag != null)
                    {
                        if (!string.IsNullOrEmpty(tag.Tag) && hashDict.ContainsKey(tag.Tag))
                        {
                            hashDict[tag.Tag] = int.Parse(hashDict[tag.Tag].ToString()) + 1;
                        }
                        else
                        {
                            hashDict[tag.Tag] = 1;
                        }
                    }

                });
            });
          
            tweetResponse.TweentTotalCount += records.Count;
            foreach (var dict in hashDict)
            {
                var key = dict.Key;
                if (tweetResponse.HashTags.ContainsKey(key))
                {
                    tweetResponse.HashTags[key] = tweetResponse.HashTags[key] + dict.Value;
                }
                else
                {
                    tweetResponse.HashTags[key] = dict.Value;
                }

            }

            tweetResponse.HashTags = tweetResponse.HashTags.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            await PublishData();
        }

        private async Task PublishData()
        {
            _producerHandler.Data = JsonConvert.SerializeObject(tweetResponse);
           await _producerHandler.ProduceAsync();       
        }
    }
}
