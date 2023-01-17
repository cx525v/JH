using Newtonsoft.Json;
using ProcessService.Interfaces;
using SharedLibrary.Constants;
using SharedLibrary.Handlers.Interfaces;
using SharedLibrary.Models;

namespace ProcessService.Services
{
    public class UpdateDataService : IUpdateDataService
    {
        readonly TweetResponse tweetResponse = new TweetResponse();

        private readonly ILogger<DataProcessService> _logger;
        private readonly IProducerBuilderHandler _producerHandler;
        public UpdateDataService(ILogger<DataProcessService> logger, IProducerBuilderHandler producerHandler)
        {
            _logger = logger;
           
            _producerHandler = producerHandler;
            _producerHandler.Topic = AppConstants.PROCESS_TOPIC;
            _producerHandler.BootstrapServers = Environment.GetEnvironmentVariable("BOOTSTRAP_SERVERS");
            _producerHandler.ProcessCompleted += _producerHandler_ProcessCompleted;
        }

        //data can be saved into database, here upsert into memory
        public async Task UpdateData(List<TwitterRecord> records)
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
            var hashtags = (from entry in tweetResponse.HashTags
                            orderby entry.Value descending
                            select entry
                   ).Take(10)
                   .ToDictionary(pair => pair.Key, pair => pair.Value);
            var data = new TweetResponse();
            data.TweentTotalCount = tweetResponse.TweentTotalCount;
            data.HashTags = hashtags;
            _producerHandler.Data = JsonConvert.SerializeObject(data);
            await _producerHandler.ProduceAsync();
        }

        private void _producerHandler_ProcessCompleted(string data)
        {
            _logger.LogInformation($"publish completed: {data}");
        }
    }
}
