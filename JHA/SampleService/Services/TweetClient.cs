using Confluent.Kafka;
using Newtonsoft.Json;
using SampleService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;
using System.Text;

namespace SampleService.Services
{
    public class TweetClient : ITweetClient
    {
        private readonly ILogger<TweetClient> _logger;
        private readonly IAppHttpClientHandler _httpClient;
        private readonly IProducerBuilderHandler _producer;
        public List<TwitterRecord> BulkRecords { get; set; } = new List<TwitterRecord>();

        public TweetClient(ILogger<TweetClient> logger, IAppHttpClientHandler httpClient, IProducerBuilderHandler producer)
        {
            _logger = logger;
            _httpClient = httpClient;
            _producer = producer;
            _producer.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _producer.Topic = AppConstants.SAMPLE_TOPIC;
            _producer.ProcessCompleted += _producer_ProcessCompleted;
        }

        private void _producer_ProcessCompleted(string data)
        {
            BulkRecords.Clear();
        }

        //get twitter sample stream
        public async Task GetTweetData(CancellationToken cancellationToken)
        {      
            using var response = await _httpClient.GetStreamAsync();
            using var sr = new StreamReader(response, Encoding.UTF8, true, 16384, leaveOpen: true);       
            while (!cancellationToken.IsCancellationRequested && !sr.EndOfStream)
            {
                var res = await sr.ReadLineAsync().ConfigureAwait(false);     
                if (!string.IsNullOrEmpty(res))
                {         
                    var record = JsonConvert.DeserializeObject<TwitterRecord>(res);
                    if(record != null)
                    {
                        await Publish(record, cancellationToken);
                    }               
                }

                if (cancellationToken.IsCancellationRequested) break;
            }

        }

  
        // produce message to tweetRawDataTopic topic
        // considering we may need to save data into database, I use batch data to have better performance. 
        private async Task Publish(TwitterRecord record, CancellationToken cancellationToken)
        {
         
            if (BulkRecords.Count <100)
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
    }
}
