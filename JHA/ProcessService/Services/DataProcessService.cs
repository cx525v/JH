using Confluent.Kafka;
using Newtonsoft.Json;
using ProcessService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Models;

namespace ProcessService.Services
{
    public class DataProcessService: IDataProcessService
    {
      
        readonly TweetResponse tweetResponse = new TweetResponse();

        private readonly ILogger<DataProcessService> _logger;
        public DataProcessService(ILogger<DataProcessService> logger)
        {
            _logger = logger;
        }

        //get data from producer with the topic
        public async Task ProcessData()
        {
            var config = new ConsumerConfig
            {
                GroupId = "test-consumer-group",
                BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS"),
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(AppConstants.SAMPLE_TOPIC);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var cr = consumer.Consume();

                            if (cr.Message.Value != null)
                            {
                                List<TwitterRecord> BulkRecords = JsonConvert.DeserializeObject<List<TwitterRecord>>(cr.Message.Value);
                                
                                await UpdateData(BulkRecords);
                            }

                        }
                        catch (ConsumeException e)
                        {
                            _logger.LogError($"Error occured: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException er)
                {
                    _logger.LogError(er.Message);
                    consumer.Close();
                }
            }
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

        public async Task PublishData()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS")
            };

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    await producer.ProduceAsync(AppConstants.PROCESS_TOPIC, new Message<Null, string> { Value = JsonConvert.SerializeObject(tweetResponse) });
                }
                catch (ProduceException<Null, string> e)
                {
                    _logger.LogError($"Delivery TweetResponse failed: {e.Error.Reason}");
                }
            }
        }
    }
}
