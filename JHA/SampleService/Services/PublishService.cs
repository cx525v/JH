using Newtonsoft.Json;
using SampleService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;

namespace SampleService.Services
{
    public class PublishService : IPublishService
    {
        public List<TwitterRecord> BulkRecords { get; set; } = new List<TwitterRecord>();

        private readonly IProducerBuilderHandler _producer;
        private readonly ILogger<PublishService> _logger;
        public PublishService(ILogger<PublishService> logger, IProducerBuilderHandler producer)
        {
            _logger = logger;
            _producer = producer;
            _producer.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _producer.Topic = AppConstants.SAMPLE_TOPIC;
            _producer.ProcessCompleted += _producer_ProcessCompleted;
        }

        // produce message to tweetRawDataTopic topic
        // considering we may need to save data into database, I use batch data to have better performance. 
        public async Task Publish(TwitterRecord record, CancellationToken cancellationToken)
        {

            if (BulkRecords.Count < 100)
            {
                BulkRecords.Add(record);
            }

            //produce 100 tweets (can be configurable) 

            if (BulkRecords.Count >= 100 || cancellationToken.IsCancellationRequested)
            {
                _producer.Data = JsonConvert.SerializeObject(BulkRecords);
                await _producer.ProduceAsync();
            }

        }


        private void _producer_ProcessCompleted(string data)
        {
            _logger.LogInformation($"publish completed: {data}");
            BulkRecords.Clear();
        }
    }
}
