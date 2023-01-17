using Confluent.Kafka;
using Newtonsoft.Json;
using SampleService.Interfaces;
using SharedLibrary.Models;
using System.Text;

namespace SampleService.Services
{
    public class TweetClient : ITweetClient
    {
        private readonly ILogger<TweetClient> _logger;
        private readonly IAppHttpClientHandler _httpClient;
        public List<TwitterRecord> BulkRecords { get; set; } = new List<TwitterRecord>();

        public TweetClient(ILogger<TweetClient> logger, IAppHttpClientHandler httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
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
                var config = new ProducerConfig
                {
                    BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")
                };

                using (var producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    try
                    {
                        await producer.ProduceAsync("tweetRawDataTopic", new Message<Null, string> { Value = JsonConvert.SerializeObject(BulkRecords) });
                        BulkRecords.Clear();
                     }
                    catch (ProduceException<Null, string> e)
                    {
                        _logger.LogError($"Delivery failed: {e.Error.Reason}");
                    }
                }
            }
           
        }
    }
}
